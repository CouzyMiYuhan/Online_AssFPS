using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    [Header("拿在手上的局部偏移（FPS 视角）")]
    public Vector3 heldLocalPosition = new Vector3(0.2f, -0.2f, 0.6f);
    public Vector3 heldLocalEulerAngles = new Vector3(0f, 0f, 0f);
    public Vector3 heldLocalScale = Vector3.one * 0.5f;

    [Header("可选：掉落时恢复用")]
    public bool rememberOriginalTransform = true;

    private Transform _originalParent;
    private Vector3 _originalLocalPos;
    private Vector3 _originalLocalEuler;
    private Vector3 _originalLocalScale;

    private bool _isHeld;

    void Awake()
    {
        if (rememberOriginalTransform)
        {
            _originalParent = transform.parent;
            _originalLocalPos = transform.localPosition;
            _originalLocalEuler = transform.localEulerAngles;
            _originalLocalScale = transform.localScale;
        }
    }

    /// <summary>
    /// 捡起时调用：挂到 handAnchor 底下，并应用局部偏移
    /// </summary>
    public void AttachToHand(Transform handAnchor)
    {
        _isHeld = true;

        // 关掉漂浮、碰撞、重力等
        var rot = GetComponent<PowerUps.Rotate>();
        if (rot) rot.enabled = false;

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.SetParent(handAnchor);
        transform.localPosition = heldLocalPosition;
        transform.localEulerAngles = heldLocalEulerAngles;
        transform.localScale = heldLocalScale;
    }

    /// <summary>
    /// 丢弃/从手上拿下来的时候调用（可选）
    /// </summary>
    public void DetachFromHand(Vector3 dropWorldPos)
    {
        _isHeld = false;

        var rot = GetComponent<PowerUps.Rotate>();
        if (rot) rot.enabled = true;

        var col = GetComponent<Collider>();
        if (col) col.enabled = true;

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }

        if (rememberOriginalTransform && _originalParent)
        {
            transform.SetParent(_originalParent);
            transform.position = dropWorldPos;
            transform.localEulerAngles = _originalLocalEuler;
            transform.localScale = _originalLocalScale;
        }
    }
}
