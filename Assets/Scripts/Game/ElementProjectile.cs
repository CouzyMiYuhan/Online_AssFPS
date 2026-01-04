using UnityEngine;
using Photon.Pun;   // ★ 新增

public class ElementProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 20f;
    public float moveDuration = 3f;      // 飞行阶段时长
    public float lingerDuration = 3f;    // 停滞阶段时长

    [Header("Damage")]
    public float damageAmount = 30f;     // 造成的伤害
    public float damageRadius = 3f;      // AoE 范围半径
    public LayerMask damageLayers;       // 会被判定伤害的层（比如 Player）

    private Vector3 _direction = Vector3.forward;
    private float _time = 0f;
    private bool _hasExploded = false;
    private Transform _owner;            // 发射者（避免打到自己）

    public void Init(Vector3 direction, Transform owner)
    {
        _direction = direction.normalized;
        _owner = owner;
    }

    void Update()
    {
        _time += Time.deltaTime;

        if (_time < moveDuration)
        {
            // 阶段1：一直往前飞
            transform.position += _direction * speed * Time.deltaTime;
        }
        else
        {
            // 刚刚进入停止阶段：只触发一次 AoE 伤害
            if (!_hasExploded)
            {
                DoAreaDamage();
                _hasExploded = true;
            }

            // 停止一段时间后销毁
            if (_time >= moveDuration + lingerDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    void DoAreaDamage()
    {
        Debug.Log("[Projectile] DoAreaDamage 调用");

        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
        Debug.Log($"[Projectile] OverlapSphere 命中数量 = {hits.Length}");

        foreach (var col in hits)
        {
            Debug.Log($"[Projectile] 命中 Collider: {col.name}, Layer = {LayerMask.LayerToName(col.gameObject.layer)}");

            // (可选) 跳过自己
            // if (_owner != null && (col.transform == _owner || col.transform.IsChildOf(_owner)))
            //     continue;

            // ★ 优先尝试网络版 CharacterHealth
            CharacterHealth ch = col.GetComponentInParent<CharacterHealth>();
            if (ch != null)
            {
                PhotonView targetView = ch.GetComponent<PhotonView>();

                if (PhotonNetwork.IsConnected && targetView != null)
                {
                    targetView.RPC("RPC_TakeDamage", RpcTarget.All, damageAmount);
                    Debug.Log($"[Projectile-Net] 对 {col.name} 造成 {damageAmount} 伤害");
                }
                else
                {
                    ch.TakeDamage(damageAmount);
                    Debug.Log($"[Projectile] 对 {col.name} 造成 {damageAmount} 伤害");
                }

                continue;
            }

            // 没有 CharacterHealth，退回 IDamageable
            IDamageable dmg = col.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damageAmount);
                Debug.Log($"[Projectile-Other] {col.name} 的父节点中 IDamageable 受到 {damageAmount} 伤害");
            }
            else
            {
                Debug.Log($"[Projectile] {col.name} 的父节点中没有 IDamageable");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
