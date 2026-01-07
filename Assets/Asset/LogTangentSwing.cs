using UnityEngine;

public class TPendulumKinematic_FixPivot : MonoBehaviour
{
    [Header("Where is the LOG CENTER relative to this GameObject pivot?")]
    public Vector3 centerLocalOffset = Vector3.zero;

    [Header("Rod definition (from LOG CENTER to A)")]
    public float rodLength = 2f;
    public Vector3 rodLocalDir = Vector3.up; // 从圆木中心指向A的方向（本地轴）

    [Header("Swing axis in WORLD")]
    public Vector3 swingAxisWorld = new Vector3(0, 0, 1); // XY平面摆动=>Z轴

    [Header("Motion")]
    public float swingAngleDeg = 90f;
    public float cycleSeconds = 2f;

    [Tooltip("Initial angle offset (deg). Example: 0 for first, 10 for second.")]
    public float startAngleDeg = 0f;

    [Tooltip("Optional: start all pendulums at time=0 but with different phase (0..1). If you use this, startAngleDeg can stay 0.")]
    [Range(0f, 1f)]
    public float startPhase01 = 0f;

    private Vector3 pivotA;
    private Vector3 offset0;
    private Quaternion rot0;

    void Start()
    {
        Vector3 centerWorld = transform.TransformPoint(centerLocalOffset);
        Vector3 rodDirWorld = transform.TransformDirection(rodLocalDir.normalized);

        pivotA = centerWorld + rodDirWorld * rodLength;
        offset0 = centerWorld - pivotA;
        rot0 = transform.rotation;

        // 可选：如果希望“开局就处在 startAngleDeg 的位置”，这里可以立刻摆到该角度
        // （否则会在第一帧 Update 才生效）
        Quaternion q0 = Quaternion.AngleAxis(ClampTheta(startAngleDeg), swingAxisWorld.normalized);
        ComputeAndApplyPose(q0);
    }

    void Update()
    {
        float t = (cycleSeconds <= 0.0001f) ? 0f : (Time.time / cycleSeconds);

        // 让每个摆锤有不同起始相位（更推荐）：t + startPhase01
        float u = Mathf.PingPong(t + startPhase01, 1f); // 0->1->0
        float baseTheta = Mathf.Lerp(-swingAngleDeg, +swingAngleDeg, u);

        // 或者用“角度偏移”：每个摆锤填不同 startAngleDeg（0,10,20...）
        float theta = ClampTheta(baseTheta + startAngleDeg);

        Quaternion q = Quaternion.AngleAxis(theta, swingAxisWorld.normalized);
        ComputeAndApplyPose(q);
    }

    private float ClampTheta(float theta)
    {
        // 防止越界
        return Mathf.Clamp(theta, -swingAngleDeg, +swingAngleDeg);
    }

    private void ComputeAndApplyPose(Quaternion q)
    {
        Quaternion newRot = q * rot0;
        Vector3 newCenterWorld = pivotA + q * offset0;
        Vector3 newPos = newCenterWorld - (newRot * centerLocalOffset);

        transform.SetPositionAndRotation(newPos, newRot);
    }
}
