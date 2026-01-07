using UnityEngine;
using Photon.Pun;

/// <summary>
/// Third-person CharacterController mover + Jump/Gravity + Animator drive (PUN2 Local Control)
/// 依赖：
/// - 同物体上有 CharacterController
/// - 玩家Prefab上有 PhotonView（建议在根节点）
/// - 子物体或本体上有 PlayerAnimatorDriver（你已使用最新版）
///
/// 联机规则：
/// - 只有 photonView.IsMine 的玩家才会读输入并移动/跳跃/攻击
/// - 远端玩家由 PhotonTransformView / PhotonAnimatorView 同步展示
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera cam;                              // 不填会自动找 Camera.main（只对本地玩家有效）
    public Transform rotateRoot;                    // 角色模型的根(只转模型不转胶囊)；不填则旋转 this.transform
    public PlayerAnimatorDriver animDriver;         // 不填会自动在子物体里找

    [Header("Move")]
    public float moveSpeed = 4.0f;
    public float sprintSpeed = 6.5f;
    public bool allowSprint = true;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Tooltip("模型转向速度（度/秒）")]
    public float rotSpeed = 720f;

    [Tooltip("加速度（越大越跟手）")]
    public float acceleration = 20f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.2f;                 // 跳跃高度（米）
    public float gravity = -20f;                    // 重力（负数；-20 比 -9.81 更像游戏）
    public float terminalVelocity = -40f;           // 下落最大速度
    public float groundedStick = -2f;               // 贴地速度（防止 grounded 抖动）

    [Header("Jump Assist")]
    [Tooltip("离地后仍允许起跳的宽容时间（秒）")]
    public float coyoteTime = 0.08f;

    [Tooltip("提前按跳跃的缓冲时间（秒）")]
    public float jumpBuffer = 0.10f;

    [Header("Attack (Optional)")]
    public bool allowAttack = false;
    public int attackMouseButton = 0;               // 0 = 左键

    [Header("Climbing")]
    public bool isClimbing = false;
    private float climbSpeed = 2.0f;
    public KeyCode climbExitKey = KeyCode.LeftControl; // 退出梯子按键（可保留）
    public KeyCode climbJumpKey = KeyCode.Space;       // ✅ 空格脱离梯子
    public float climbExitPush = 0.6f;                 // ✅ 脱离时推开距离，防止立刻又进梯子
    public float climbExitUp = 0.3f;                   // ✅ 脱离时给一点上抬（可设0）
    private Ladder currentLadder;
    private bool canClimb = false;
    private Vector3 ladderTopPosition;

    private CharacterController cc;

    // 当前水平速度（用于加速度平滑）
    private Vector3 planarVelocity;

    // 垂直速度
    private float verticalVelocity;

    // coyote / buffer 计时
    private float coyoteCounter;
    private float jumpBufferCounter;

    // === PUN2 ===
    private PhotonView pv;
    private bool isLocal;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // 兼容 PhotonView 不在同一层（有些人把脚本挂子物体）
        pv = GetComponent<PhotonView>();
        if (pv == null) pv = GetComponentInParent<PhotonView>();

        // ✅ 没有 PhotonView：当作离线单机也能跑
        // ✅ 有 PhotonView：只有本地拥有者能控制
        isLocal = (pv == null) || pv.IsMine;

        if (!isLocal)
        {
            // 远端玩家：不读输入、不驱动移动/跳跃/攻击
            // 位置/动画应该由 PhotonTransformView / PhotonAnimatorView 同步
            enabled = false;
            return;
        }

        if (cam == null) cam = Camera.main;
        if (rotateRoot == null) rotateRoot = transform;
        if (animDriver == null) animDriver = GetComponentInChildren<PlayerAnimatorDriver>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLocal) return;

        if (other.CompareTag("Ladder"))
        {
            Debug.Log("Entered Ladder Trigger");
            currentLadder = other.GetComponent<Ladder>();
            canClimb = true;

            // 获取梯子顶部位置（假设梯子对象有 Ladder 组件）
            Ladder ladder = other.GetComponent<Ladder>();
            if (ladder != null)
            {
                ladderTopPosition = ladder.GetTopPosition();
            }

            StartClimbing();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isLocal) return;

        if (other == currentLadder)
        {
            Debug.Log("Exited Ladder Trigger");
            canClimb = false;
            currentLadder = null;
            if (isClimbing)
            {
                StopClimbing();
            }
        }
    }

    void Update()
    {
        if (!isLocal) return;
        if (cam == null) return;

        // ====== Climbing Logic (提前处理) ======
        HandleClimbing();

        // 如果正在爬梯子，跳过常规移动、跳跃和旋转逻辑
        if (isClimbing)
        {
            // 可选：更新动画参数
            float verticalInput = Input.GetAxisRaw("Vertical");
            if (animDriver != null)
            {
                animDriver.SetMoveSpeed(Mathf.Abs(verticalInput)); // 使用垂直输入作为移动速度
            }
            return; // 跳过剩下的常规移动逻辑
        }

        // ====== Input ======
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(h, v);
        float inputMag = Mathf.Clamp01(input.magnitude);

        // 跳跃输入缓冲
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBuffer;
        else
            jumpBufferCounter -= Time.deltaTime;

        // 可选攻击输入
        if (allowAttack && Input.GetMouseButtonDown(attackMouseButton))
        {
            if (animDriver != null) animDriver.TriggerAttack();
        }

        // ====== Grounded / Coyote ======
        bool grounded = cc.isGrounded;

        if (grounded)
        {
            coyoteCounter = coyoteTime;

            // 贴地，避免落地瞬间 verticalVelocity 仍为负导致“黏不住地/跳不稳”
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStick;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // ====== Build camera-relative move direction ======
        Vector3 camForward = cam.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0f; camRight.Normalize();

        Vector3 desiredDir = (camForward * v + camRight * h);
        if (desiredDir.sqrMagnitude > 0.0001f) desiredDir.Normalize();

        // ====== Speed ======
        float targetSpeed = moveSpeed;

        if (allowSprint && Input.GetKey(sprintKey) && inputMag > 0.01f)
            targetSpeed = sprintSpeed;

        Vector3 targetPlanarVel = desiredDir * targetSpeed;

        // 加速度平滑
        planarVelocity = Vector3.MoveTowards(planarVelocity, targetPlanarVel, acceleration * Time.deltaTime);

        // ====== Jump execute ======
        // 条件：在地面 or coyoteTime 内 + 有 jumpBuffer
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // 这帧真正起跳 -> 触发 Jump 动画
            if (animDriver != null) animDriver.TriggerJump();

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // ====== Gravity ======
        verticalVelocity += gravity * Time.deltaTime;
        if (verticalVelocity < terminalVelocity) verticalVelocity = terminalVelocity;

        // ====== Move ======
        Vector3 motion = planarVelocity;
        motion.y = verticalVelocity;

        cc.Move(motion * Time.deltaTime);

        // ====== Rotate model ======
        // 只在有水平速度时转向（保持你原逻辑不变）
        Vector3 faceDir = new Vector3(planarVelocity.x, 0f, planarVelocity.z);
        if (faceDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(faceDir);
            rotateRoot.rotation = Quaternion.RotateTowards(rotateRoot.rotation, targetRot, rotSpeed * Time.deltaTime);
        }

        // ====== Animator: MoveSpeed ======
        if (animDriver != null)
            animDriver.SetMoveSpeed(inputMag);
    }

    void HandleClimbing()
    {
        if (!isClimbing) return;

        // ✅ 空格 / Ctrl 退出梯子
        if (Input.GetKeyDown(climbJumpKey) || Input.GetKeyDown(climbExitKey))
        {
            // 先退出 climbing 状态并恢复 CC
            StopClimbing();

            // 推开梯子触发器：沿角色面朝方向推一下 + 稍微上抬
            Vector3 pushDir = rotateRoot != null ? rotateRoot.forward : transform.forward;
            pushDir.y = 0f;
            if (pushDir.sqrMagnitude < 0.0001f) pushDir = transform.forward;
            pushDir.Normalize();

            // 直接改 transform（你现在爬梯子阶段本来也在直接改 transform）
            transform.position += pushDir * climbExitPush + Vector3.up * climbExitUp;

            return;
        }

        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = Vector3.up * v * climbSpeed * Time.deltaTime;
        transform.position += move;

        // 到达顶部
        if (transform.position.y >= ladderTopPosition.y)
        {
            transform.position = new Vector3(
                transform.position.x,
                ladderTopPosition.y,
                transform.position.z
            );
            StopClimbing();
            return;
        }

        if (animDriver != null)
            animDriver.SetClimbingSpeed(Mathf.Abs(v));
    }


    void StartClimbing()
    {
        isClimbing = true;

        planarVelocity = Vector3.zero;
        verticalVelocity = 0f;

        // 🔴 关键：冻结 CharacterController
        cc.enabled = false;

        // 对齐到梯子中心
        //if (currentLadder != null)
        //{
        //    Vector3 center = currentLadder.bounds.center;
        //    transform.position = new Vector3(
        //        center.x,
        //        transform.position.y,
        //        center.z
        //    );
        //}

        if (currentLadder != null)
        {
            if (currentLadder.startPos != null)
            {
                if (currentLadder != null)
                {
                    Transform chosen = currentLadder.startPos;

                    if (currentLadder.startPos != null && currentLadder.endPos != null)
                    {
                        float dyStart = Mathf.Abs(transform.position.y - currentLadder.startPos.position.y);
                        float dyEnd = Mathf.Abs(transform.position.y - currentLadder.endPos.position.y);
                        chosen = (dyEnd < dyStart) ? currentLadder.endPos : currentLadder.startPos;
                    }

                    if (chosen != null)
                    {
                        transform.position = chosen.position;
                        transform.rotation = chosen.rotation;
                    }
                }

            }
        }

        if (animDriver != null)
            animDriver.StartClimbing();
    }


    void StopClimbing()
    {
        isClimbing = false;
        canClimb = false;

        // ⚠️ 先恢复 CC，再给一个轻微下压
        cc.enabled = true;

        // 防止重新启用 CC 时弹飞 / 穿地
        cc.Move(Vector3.down * 0.1f);

        if (animDriver != null)
            animDriver.LeaveClimb();

        if (currentLadder != null)
        {
            if (currentLadder.endPos != null)
            {
                transform.position = currentLadder.endPos.position;
                transform.rotation = currentLadder.endPos.rotation;
            }
        }
        currentLadder = null;
    }
}
