using UnityEngine;
using Photon.Pun;   // ★ 新增

public class DirectHitProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 10f;
    public LayerMask hitLayers;
    public ElementType elementType;

    private Vector3 _direction = Vector3.forward;
    private Transform _owner;
    private float _time = 0f;

    public void Init(Vector3 direction, Transform owner)
    {
        _direction = direction.normalized;
        _owner = owner;
    }

    void Update()
    {
        _time += Time.deltaTime;
        transform.position += _direction * speed * Time.deltaTime;

        if (_time >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 不打到自己
        if (_owner != null && (other.transform == _owner || other.transform.IsChildOf(_owner)))
            return;

        // 如果设置了 hitLayers，就按层过滤一下
        if (hitLayers.value != 0 && (hitLayers & (1 << other.gameObject.layer)) == 0)
            return;

        // ★ 优先尝试网络版 CharacterHealth
        CharacterHealth ch = other.GetComponentInParent<CharacterHealth>();
        if (ch != null)
        {
            PhotonView targetView = ch.GetComponent<PhotonView>();

            if (PhotonNetwork.IsConnected && targetView != null)
            {
                // 联机：所有人一起扣血
                targetView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
                Debug.Log($"[DirectHit-Net] Hit {other.name} for {damage}, element = {elementType}");

                // 持续伤害状态也加在这边（所有客户端都会加一次）
                if (elementType == ElementType.Water)
                {
                    var burn = ch.GetComponent<BurningStatus>();
                    if (burn == null) burn = ch.gameObject.AddComponent<BurningStatus>();
                    burn.Refresh();
                }
            }
            else
            {
                // 单机 / 没有 PhotonView：走原本单机逻辑
                ch.TakeDamage(damage);
                Debug.Log($"[DirectHit] Hit {other.name} for {damage}, element = {elementType}");

                if (elementType == ElementType.Water)
                {
                    var burn = ch.GetComponent<BurningStatus>();
                    if (burn == null) burn = ch.gameObject.AddComponent<BurningStatus>();
                    burn.Refresh();
                }
            }

            Destroy(gameObject);
            return;
        }

        // ★ 如果不是玩家，可以保持原来的 IDamageable 逻辑（比如以后有木桶之类）
        IDamageable dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            Debug.Log($"[DirectHit-Other] Hit {other.name} for {damage}, element = {elementType}");
        }

        Destroy(gameObject);
    }
}
