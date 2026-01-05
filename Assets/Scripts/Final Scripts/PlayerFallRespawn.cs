using UnityEngine;
using Photon.Pun; // ✅ 新增

public class PlayerFallRespawn : MonoBehaviourPun
{
    public float deathY = -5f;

    [Tooltip("场景中的 OrderedCheckpointManager（可以不拖，运行时会自动找）")]
    public OrderedCheckpointManager checkpointManager;

    public float respawnHeightOffset = 1.5f;

    private CharacterController cc;
    private Rigidbody rb;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        // ✅ 网络生成的玩家通常拿不到场景引用，所以这里自动找
        if (checkpointManager == null)
            checkpointManager = FindObjectOfType<OrderedCheckpointManager>();
    }

    private void Update()
    {
        // ✅ 只让本机玩家复活，远端副本不允许改位置
        if (!photonView.IsMine) return;

        if (transform.position.y < deathY)
            RespawnToLastOrFirst();
    }

    private void RespawnToLastOrFirst()
    {
        if (checkpointManager == null)
            checkpointManager = FindObjectOfType<OrderedCheckpointManager>();
        if (checkpointManager == null) return;

        OrderedCheckpoint targetCp =
            (checkpointManager.LastPassedCheckpoint != null)
                ? checkpointManager.LastPassedCheckpoint
                : checkpointManager.DefaultRespawnCheckpoint;

        if (targetCp == null) return;

        // doorPlasmaObject 可能被 SetActive(false)，但 transform 仍然可用
        Transform door = (targetCp.doorPlasmaObject != null)
            ? targetCp.doorPlasmaObject.transform
            : targetCp.transform;

        Vector3 targetPos = door.position + Vector3.up * respawnHeightOffset;

        if (cc != null) cc.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = targetPos;

        if (cc != null) cc.enabled = true;
    }
}
