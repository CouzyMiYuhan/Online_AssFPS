using Photon.Pun;
using UnityEngine;

public class PlayerSetup1 : MonoBehaviourPun
{
    public Camera playerCamera;        // 如果你把相机做成 Player 的子节点
    public AudioListener audioListener;

    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            // ★ 远程玩家：把自己的相机和 AudioListener 关掉，防止多相机、多声音
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
        }
        else
        {
            // ★ 本地玩家：设置全局的 Camera.main 之类
            if (playerCamera != null)
            {
                playerCamera.gameObject.tag = "MainCamera";
            }
        }
    }
}
