using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimplePortal : MonoBehaviour
{
    [Header("目标传送门")]
    public SimplePortal targetPortal;

    [Header("落点相对目标门前方偏移")]
    public float forwardOffset = 2f;

    [Header("Audio")]
    public AudioSource sfxSource;     // 门自己的 AudioSource（可选）
    public AudioClip teleportClip;   // 传送音效
    public float volume = 1f;


    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"[SimplePortal:{name}] Awake → 自动把 Collider.isTrigger 设为 true");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Teleportable tp = other.GetComponentInParent<Teleportable>();
        if (tp == null) return;
        if (!tp.CanTeleport()) return;

        if (targetPortal == null)
        {
            Debug.LogError($"[SimplePortal:{name}] targetPortal 为空！");
            return;
        }

        Transform root = tp.transform;
        Transform dstT = targetPortal.transform;

        // ★ 传送目标 = 目标门位置 + 朝向前方偏移
        Vector3 dstPos = dstT.position + dstT.forward * forwardOffset;

        Vector3 before = root.position;

        var cc = root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        root.position = dstPos;
        root.rotation = dstT.rotation;

        if (cc != null) cc.enabled = true;

        Vector3 after = root.position;
        float dist = Vector3.Distance(before, after);
        Debug.Log($"[SimplePortal:{name}] Teleport '{root.name}' from {before} to {after}, dist={dist:F2}");

        PlayTeleportSfx();

        tp.MarkTeleported();
    }
    void PlayTeleportSfx()
    {
        if (teleportClip == null) return;

        // 优先用门身上的 AudioSource
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(teleportClip, volume);
        }
        else
        {
            // 没有绑定就临时用世界音效
            AudioSource.PlayClipAtPoint(teleportClip, transform.position, volume);
        }
    }

}
