using Photon.Pun;
using UnityEngine;

public class PlayerSkillCastAnimator : MonoBehaviourPun
{
    [Header("Animator")]
    public Animator animator;
    public string castTriggerName = "CastSkill";

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 本地玩家在“成功释放技能”时调用一次即可
    /// </summary>
    public void PlayCast()
    {
        if (!photonView.IsMine) return;

        // 广播给所有人（包括自己）播放施法动画
        photonView.RPC(nameof(RPC_PlayCast), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayCast()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        animator.ResetTrigger(castTriggerName);
        animator.SetTrigger(castTriggerName);
    }
}
