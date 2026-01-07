using Photon.Pun;
using UnityEngine;

public class PlayerSlowOrbSkill : MonoBehaviourPun, ISkillCooldownReadable
{
    public KeyCode castKey = KeyCode.E;

    [Header("UI")]
    public Sprite skillIcon;

    [Header("Network Prefab (Resources/)")]
    public string orbPrefabName = "Skill_SlowOrb";

    [Header("Spawn")]
    public Transform castOrigin;
    public float spawnForward = 1.2f;
    public float spawnUp = 0.2f;

    [Header("Cooldown")]
    public float cooldown = 6f;
    private float nextReady = 0f;

    // ====== ISkillCooldownReadable ======
    public float CooldownDuration => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, nextReady - Time.time);
    public bool IsReady => Time.time >= nextReady;
    public Sprite Icon => skillIcon;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(castKey) && IsReady)
        {
            nextReady = Time.time + cooldown;
            Cast();
        }
    }

    private void Cast()
    {
        Transform o = castOrigin != null ? castOrigin : transform;

        Vector3 fwd = o.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;
        fwd.Normalize();

        Vector3 pos = o.position + fwd * spawnForward + Vector3.up * spawnUp;
        Quaternion rot = Quaternion.LookRotation(fwd, Vector3.up);

        GetComponent<PlayerSkillCastAnimator>()?.PlayCast();

        PhotonNetwork.Instantiate(orbPrefabName, pos, rot);
    }
}
