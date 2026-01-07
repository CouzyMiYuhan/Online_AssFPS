using Photon.Pun;
using UnityEngine;
using System.Collections;

public class PlayerSpeedBoostSkill : MonoBehaviourPun, ISkillCooldownReadable
{
    [Header("Input")]
    public KeyCode castKey = KeyCode.E;

    [Header("UI")]
    public Sprite skillIcon;

    [Header("Refs")]
    public PlayerController controller;   // 不填会自动找

    [Header("Boost")]
    public float boostMultiplier = 1.6f;  // 1.6 = 速度*1.6
    public float duration = 10f;          // 持续10秒
    public float cooldown = 12f;          // 冷却

    [Header("Trail VFX (Prefab, 不需要PhotonView)")]
    public GameObject trailVfxPrefab;
    public Transform vfxAttach;
    public Vector3 vfxLocalPos = new Vector3(0f, 0.9f, -0.6f);
    public Vector3 vfxLocalEuler = Vector3.zero;

    private float _nextReadyTime = 0f;
    private Coroutine _co;

    private float _baseMoveSpeed;
    private float _baseSprintSpeed;
    private bool _cachedBase = false;

    // ====== ISkillCooldownReadable ======
    public float CooldownDuration => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, _nextReadyTime - Time.time);
    public bool IsReady => Time.time >= _nextReadyTime;
    public Sprite Icon => skillIcon;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (controller == null) controller = GetComponentInParent<PlayerController>();
        CacheBaseIfNeeded();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(castKey) && IsReady)
        {
            _nextReadyTime = Time.time + cooldown;

            // 技能动画（同步）
            GetComponent<PlayerSkillCastAnimator>()?.PlayCast();

            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(CoBoost());
        }
    }

    private void CacheBaseIfNeeded()
    {
        if (_cachedBase) return;
        if (controller == null) return;

        _baseMoveSpeed = controller.moveSpeed;
        _baseSprintSpeed = controller.sprintSpeed;
        _cachedBase = true;
    }

    private IEnumerator CoBoost()
    {
        if (controller == null)
        {
            Debug.LogWarning("[PlayerSpeedBoostSkill] PlayerController not found on this player.");
            yield break;
        }

        CacheBaseIfNeeded();

        // 1) 本机真正加速：直接改你的字段
        controller.moveSpeed = _baseMoveSpeed * boostMultiplier;
        controller.sprintSpeed = _baseSprintSpeed * boostMultiplier;

        // 2) 所有人看到拖尾
        photonView.RPC(nameof(RPC_PlayTrail), RpcTarget.All, duration);

        yield return new WaitForSeconds(duration);

        // 3) 恢复
        controller.moveSpeed = _baseMoveSpeed;
        controller.sprintSpeed = _baseSprintSpeed;

        _co = null;
    }

    [PunRPC]
    private void RPC_PlayTrail(float life)
    {
        if (trailVfxPrefab == null) return;

        Transform attach = vfxAttach;

        // 默认：挂在 rotateRoot（更跟随模型方向），否则挂在自身
        if (attach == null)
        {
            if (controller != null && controller.rotateRoot != null) attach = controller.rotateRoot;
            else attach = transform;
        }

        var go = Instantiate(trailVfxPrefab, attach);
        go.transform.localPosition = vfxLocalPos;
        go.transform.localRotation = Quaternion.Euler(vfxLocalEuler);

        Destroy(go, life + 0.2f);
    }
}
