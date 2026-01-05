using UnityEngine;
using Photon.Pun; //

[RequireComponent(typeof(Rigidbody))]
public class OrderedCheckpoint : MonoBehaviour
{
    [Header("如果不拖，默认按子物体名 DoorPlasma_02 自动找")]
    public GameObject doorPlasmaObject;

    [Header("Player Detection")]
    public string playerTag = "Player";

    public int Index { get; private set; } = -1;

    private OrderedCheckpointManager manager;
    private Rigidbody rb;

    private bool isCurrent = false;

    public void Bind(OrderedCheckpointManager m, int index)
    {
        manager = m;
        Index = index;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (doorPlasmaObject == null)
        {
            var t = transform.Find("DoorPlasma_02");
            if (t != null) doorPlasmaObject = t.gameObject;
        }
    }

    public void SetCurrent(bool current)
    {
        isCurrent = current;
        if (doorPlasmaObject != null)
            doorPlasmaObject.SetActive(current);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCurrent) return;
        if (!other.CompareTag(playerTag)) return;

        // 
        PhotonView pv = other.GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine) return;

        // 通过当前检查点：本机隐藏该门（只影响本机视觉）
        if (doorPlasmaObject != null)
            doorPlasmaObject.SetActive(false);

        manager?.NotifyReached(this);
    }
}
