using UnityEngine;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

public class LocalPlayerMarker : MonoBehaviour
{
#if PHOTON_UNITY_NETWORKING
    private void Awake()
    {
        var pv = GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine)
            enabled = false;
    }
#endif
}
