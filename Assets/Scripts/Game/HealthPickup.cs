using UnityEngine;
using Photon.Pun;   // ★ 新增

[RequireComponent(typeof(Collider))]
public class HealthPickup : MonoBehaviourPun
{
    [Header("回血量")]
    public float healAmount = 100f;

    [Header("只生效一次就销毁")]
    public bool destroyOnPickup = true;

    private bool used = false;

    private void Reset()
    {
        // 确保是触发器
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;

        // 找到身上或父物体的 CharacterHealth
        CharacterHealth hp = other.GetComponentInParent<CharacterHealth>();
        if (hp == null) return;

        PhotonView targetView = hp.GetComponent<PhotonView>();

        // ★ 先网络回血
        if (PhotonNetwork.IsConnected && targetView != null)
        {
            targetView.RPC("RPC_Heal", RpcTarget.All, healAmount);
            Debug.Log($"[HealthPickup-Net] {other.name} 回复 {healAmount} 点生命值");
        }
        else
        {
            // 单机 / 没 PhotonView 时退回本地
            hp.Heal(healAmount);
            Debug.Log($"[HealthPickup] {other.name} 回复 {healAmount} 点生命值");
        }

        // ★ 再网络销毁血包本体
        if (destroyOnPickup)
        {
            if (PhotonNetwork.IsConnected && photonView != null)
            {
                photonView.RPC("RPC_Consume", RpcTarget.All);
            }
            else
            {
                ConsumeLocal();
            }
        }
    }

    [PunRPC]
    private void RPC_Consume()
    {
        ConsumeLocal();
    }

    private void ConsumeLocal()
    {
        if (used) return;
        used = true;
        Destroy(gameObject);
    }
}
