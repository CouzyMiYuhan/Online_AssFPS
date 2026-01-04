using UnityEngine;

public class Teleportable : MonoBehaviour
{
    [Tooltip("同一个对象两次传送之间的冷却时间，避免来回弹。")]
    public float teleportCooldown = 0.5f;

    private float _lastTeleportTime = -999f;

    public bool CanTeleport()
    {
        bool ok = Time.time - _lastTeleportTime >= teleportCooldown;
        Debug.Log($"[Teleportable:{name}] CanTeleport? {ok} (delta={Time.time - _lastTeleportTime:F2})");
        return ok;
    }

    public void MarkTeleported()
    {
        _lastTeleportTime = Time.time;
        Debug.Log($"[Teleportable:{name}] MarkTeleported at t={_lastTeleportTime:F2}");
    }
}
