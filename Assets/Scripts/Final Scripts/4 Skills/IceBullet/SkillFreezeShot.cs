using Photon.Pun;
using UnityEngine;

public class SkillFreezeShot : MonoBehaviourPun
{
    [Header("Move")]
    public float speed = 18f;
    public float lifeTime = 1.5f;

    [Header("Hit Detect")]
    public float hitRadius = 0.6f;
    public LayerMask playerLayer;
    public QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Ignore;

    [Header("Freeze")]
    public float freezeDuration = 3f;

    private float dieAt;

    private void Start()
    {
        dieAt = Time.time + lifeTime;
    }

    private void Update()
    {
        // 只让施法者那台机推进和判定，避免重复命中
        if (!photonView.IsMine) return;

        // 前进
        transform.position += transform.forward * speed * Time.deltaTime;

        // 命中判定
        var cols = Physics.OverlapSphere(transform.position, hitRadius, playerLayer, triggerMode);
        foreach (var c in cols)
        {
            var pv = c.GetComponentInParent<PhotonView>();
            if (pv == null) continue;

            // 不打自己
            if (pv.Owner != null && photonView.Owner != null &&
                pv.Owner.ActorNumber == photonView.Owner.ActorNumber)
                continue;

            var receiver = pv.GetComponent<PlayerFreezeReceiver>();
            if (receiver == null) continue;

            // 让目标自己的本机冻结
            pv.RPC(nameof(PlayerFreezeReceiver.RPC_Freeze), pv.Owner, freezeDuration);

            // 命中即销毁（只冻到第一个碰到的人）
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        // 超时销毁
        if (Time.time >= dieAt)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
