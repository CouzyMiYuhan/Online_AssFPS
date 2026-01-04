using UnityEngine;

/// <summary>
/// 挂在“拿在手上的展示用 prefab”上，用来控制它在手上的位置/角度/缩放
/// </summary>
public class HoldPose : MonoBehaviour
{
    [Header("拿在手上的局部偏移（相对 PlayerElementHandler 的 holdPoint）")]
    public Vector3 heldLocalPosition = new Vector3(0.2f, -0.25f, 0.6f);
    public Vector3 heldLocalEulerAngles = new Vector3(0f, 0f, 0f);
    public Vector3 heldLocalScale = Vector3.one * 0.5f;

    /// <summary>
    /// 被 PlayerElementHandler 装备时调用
    /// </summary>
    public void ApplyPose(Transform parent)
    {
        // 1) 挂到手上的挂点（holdPoint）下面
        transform.SetParent(parent);

        // 2) 按偏移来放
        transform.localPosition = heldLocalPosition;
        transform.localEulerAngles = heldLocalEulerAngles;
        transform.localScale = heldLocalScale;

        // 3) 地上用的特效/物理关掉，避免在手上乱飞乱撞
        var rotate = GetComponent<PowerUps.Rotate>();
        if (rotate) rotate.enabled = false;

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}
