using Photon.Pun;
using UnityEngine;
using Cinemachine;

public class DisableRemoteFreeLook : MonoBehaviourPun
{
    void Awake()
    {
        if (photonView.IsMine) return;

        // 远端：禁用所有虚拟相机（FreeLook/VirtualCamera）
        foreach (var vcam in GetComponentsInChildren<CinemachineVirtualCameraBase>(true))
            vcam.enabled = false;
    }
}
