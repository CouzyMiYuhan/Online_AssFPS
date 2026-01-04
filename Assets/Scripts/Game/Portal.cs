using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [Header("目标传送门")]
    public Portal targetPortal;

    [Header("传送到对方的哪个点")]
    public Transform exitPoint;   // 可以留空，默认用 targetPortal 的 transform

    [Header("Audio")]
    public AudioSource sfxSource;     // 门自己的 AudioSource（可选）
    public AudioClip teleportClip;   // 传送音效
    public float volume = 1f;


    private void Reset()
    {
        // 确保 Collider 是触发器
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Teleportable tp = other.GetComponentInParent<Teleportable>();
        if (tp == null) return;
        if (!tp.CanTeleport()) return;

        // ★ 暂时忽略 targetPortal，直接把它传到天上 20 米
        Transform root = tp.transform;

        Vector3 before = root.position;
        Vector3 targetPos = before + Vector3.up * 20f;   // 往上 20 米

        var cc = root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        root.position = targetPos;

        if (cc != null) cc.enabled = true;

        Vector3 after = root.position;
        float dist = Vector3.Distance(before, after);
        Debug.Log($"[Portal:{name}] TEST Teleport '{root.name}' from {before} to {after}, dist={dist:F2}");

        tp.MarkTeleported();
    }


}
