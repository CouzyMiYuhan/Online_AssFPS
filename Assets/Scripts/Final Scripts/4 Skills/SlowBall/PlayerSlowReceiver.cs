using Photon.Pun;
using UnityEngine;
using System.Collections;

public class PlayerSlowReceiver : MonoBehaviourPun
{
    public PlayerController controller; // 你的移动脚本

    private float baseMoveSpeed;
    private float baseSprintSpeed;
    private bool cached = false;

    private float slowFactor = 1f;
    private float slowUntil = 0f;
    private Coroutine co;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (controller == null) controller = GetComponentInParent<PlayerController>();
        CacheBase();
    }

    private void CacheBase()
    {
        if (cached || controller == null) return;
        baseMoveSpeed = controller.moveSpeed;
        baseSprintSpeed = controller.sprintSpeed;
        cached = true;
    }

    [PunRPC]
    public void RPC_ApplySlow(float factor, float duration)
    {
        if (!photonView.IsMine) return; // 只在被影响者自己的本机生效

        CacheBase();
        if (controller == null) return;

        // 多次刷新：取更慢的 factor + 延长持续时间
        slowFactor = Mathf.Min(slowFactor, Mathf.Clamp(factor, 0.05f, 1f));
        slowUntil = Mathf.Max(slowUntil, Time.time + Mathf.Max(0.05f, duration));

        if (co == null) co = StartCoroutine(CoSlow());
    }

    private IEnumerator CoSlow()
    {
        while (Time.time < slowUntil)
        {
            controller.moveSpeed = baseMoveSpeed * slowFactor;
            controller.sprintSpeed = baseSprintSpeed * slowFactor;
            yield return null;
        }

        // 恢复
        controller.moveSpeed = baseMoveSpeed;
        controller.sprintSpeed = baseSprintSpeed;

        slowFactor = 1f;
        slowUntil = 0f;
        co = null;
    }
}
