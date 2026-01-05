using Photon.Pun;
using UnityEngine;

public class SkillSlowOrb : MonoBehaviourPun
{
    [Header("Move: forward 2s then stop 5s")]
    public float moveSpeed = 6f;
    public float moveTime = 2f;
    public float stopTime = 5f;

    [Header("Slow Area")]
    public float radius = 3.5f;
    public LayerMask playerLayer;
    public QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Ignore;

    [Range(0.05f, 1f)]
    public float slowFactor = 0.6f;

    [Tooltip("为了实现“离开范围就很快恢复”，每次只给一个很短的减速，持续靠刷新维持")]
    public float slowPulseDuration = 0.5f;

    [Tooltip("刷新频率")]
    public float tickInterval = 0.25f;

    [Header("Visual (可选：把球VFX子物体拖进来用于缩放)")]
    public Transform visualRoot;
    public float visualDiameterAtScale1 = 1f;

    private float spawnTime;
    private float dieTime;
    private float nextTick;

    private void Awake()
    {
        spawnTime = Time.time;
        dieTime = spawnTime + moveTime + stopTime;
        nextTick = Time.time;
        UpdateVisualScale();
    }

    private void OnValidate()
    {
        UpdateVisualScale();
    }

    private void UpdateVisualScale()
    {
        if (visualRoot == null) return;
        float d = Mathf.Max(0.01f, visualDiameterAtScale1);
        float targetDiameter = radius * 2f;
        float s = targetDiameter / d;
        visualRoot.localScale = Vector3.one * s;
    }

    private void Update()
    {
        // 只让拥有者推动移动&判定，避免每台机重复给减速
        if (!photonView.IsMine) return;

        float t = Time.time - spawnTime;

        // 1) 移动阶段：水平前进 moveTime 秒
        if (t < moveTime)
        {
            Vector3 fwd = transform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
            fwd.Normalize();

            transform.position += fwd * moveSpeed * Time.deltaTime;
        }
        // 2) 停止阶段：不动（自动停 5 秒）

        // 3) 生命周期结束 -> 网络销毁
        if (Time.time >= dieTime)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        // 4) 减速判定：tick 刷新
        if (Time.time < nextTick) return;
        nextTick = Time.time + tickInterval;

        var cols = Physics.OverlapSphere(transform.position, radius, playerLayer, triggerMode);
        foreach (var c in cols)
        {
            var pv = c.GetComponentInParent<PhotonView>();
            if (pv == null) continue;

            var receiver = pv.GetComponent<PlayerSlowReceiver>();
            if (receiver == null) continue;

            // 只让“被影响者的Owner”真正变慢
            pv.RPC(nameof(PlayerSlowReceiver.RPC_ApplySlow), pv.Owner, slowFactor, slowPulseDuration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
