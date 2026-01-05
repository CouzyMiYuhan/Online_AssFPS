using System.Collections.Generic;
using UnityEngine;

public class OrderedCheckpointManager : MonoBehaviour
{
    [Header("按顺序拖拽")]
    public List<OrderedCheckpoint> checkpoints = new List<OrderedCheckpoint>();

    [SerializeField] private int currentIndex = 0;

    // 最近一次“成功通过”的检查点（通过后门会消失的那个）
    public OrderedCheckpoint LastPassedCheckpoint { get; private set; } = null;

    // ★ 新增：默认复活点（开局没过任何门时用）
    public OrderedCheckpoint DefaultRespawnCheckpoint
        => (checkpoints != null && checkpoints.Count > 0) ? checkpoints[0] : null;

    private void Awake()
    {
        for (int i = 0; i < checkpoints.Count; i++)
            checkpoints[i].Bind(this, i);
    }

    private void Start()
    {
        ApplyVisualState();
    }

    public void ResetSequence()
    {
        currentIndex = 0;
        LastPassedCheckpoint = null;
        ApplyVisualState();
    }

    public void NotifyReached(OrderedCheckpoint who)
    {
        if (who == null) return;
        if (who.Index != currentIndex) return;

        // 记录“刚刚通过”的检查点（用于掉落复活）
        LastPassedCheckpoint = who;

        currentIndex++;
        ApplyVisualState();

        if (currentIndex >= checkpoints.Count)
        {
            Debug.Log("[Checkpoint] Finished all checkpoints.");
            // TODO: 完成逻辑（胜利）
        }
    }

    private void ApplyVisualState()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            bool isCurrent = (i == currentIndex);
            checkpoints[i].SetCurrent(isCurrent);
        }
    }
}
