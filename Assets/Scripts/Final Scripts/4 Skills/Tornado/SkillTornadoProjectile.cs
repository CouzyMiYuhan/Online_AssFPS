using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class SkillTornadoProjectile : MonoBehaviourPun
{
    [Header("Move")]
    public float speed = 10f;
    public float lifeTime = 3.5f;

    [Header("Hit Detect")]
    public float hitRadius = 1.2f;
    public LayerMask playerLayer;
    public QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Collide;

    [Header("Knockback")]
    public float pushSpeed = 6f;      // 水平推开速度
    public float upSpeed = 8f;        // 向上击飞速度（越大飞越高）
    public float knockDuration = 0.35f;

    private float _dieAt;
    private readonly HashSet<int> _hitViewIds = new HashSet<int>();

    private void Start()
    {
        _dieAt = Time.time + lifeTime;
    }

    private void Update()
    {
        // 只让“这个龙卷风的拥有者（施法者）”做判定，避免重复击中
        if (!photonView.IsMine) return;

        // 前进
        transform.position += transform.forward * speed * Time.deltaTime;

        // 命中检测
        Collider[] cols = Physics.OverlapSphere(transform.position, hitRadius, playerLayer, triggerMode);
        for (int i = 0; i < cols.Length; i++)
        {
            var pv = cols[i].GetComponentInParent<PhotonView>();
            if (pv == null) continue;

            // 不打施法者自己
            if (pv.Owner != null && photonView.Owner != null && pv.Owner.ActorNumber == photonView.Owner.ActorNumber)
                continue;

            // 同一目标只命中一次
            if (_hitViewIds.Contains(pv.ViewID)) continue;

            var receiver = pv.GetComponent<PlayerKnockbackReceiver>();
            if (receiver == null) continue;

            _hitViewIds.Add(pv.ViewID);

            // 方向：从龙卷风中心推开 + 向上
            Vector3 dir = (pv.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            Vector3 vel = dir * pushSpeed + Vector3.up * upSpeed;

            // 只让“被击中的玩家自己的那台机器（Owner）”执行击飞
            pv.RPC(nameof(PlayerKnockbackReceiver.RPC_Knockback), pv.Owner, vel, knockDuration);
        }

        // 生命周期结束：网络销毁
        if (Time.time >= _dieAt)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
