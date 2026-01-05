using Photon.Pun;
using UnityEngine;

public class PlayerFreezeShotSkill : MonoBehaviourPun
{
    public KeyCode castKey = KeyCode.E;

    [Header("Network Prefab (Resources/)")]
    public string freezeProjectilePrefab = "Skill_FreezeShot";

    [Header("Spawn")]
    public Transform castOrigin;      // 不填就用自己
    public float spawnForward = 1.1f;
    public float spawnUp = 1.0f;

    [Header("Cooldown")]
    public float cooldown = 3.0f;
    private float nextReady = 0f;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(castKey) && Time.time >= nextReady)
        {
            nextReady = Time.time + cooldown;
            Cast();
        }
    }

    private void Cast()
    {
        Transform o = castOrigin != null ? castOrigin : transform;

        Vector3 fwd = o.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;
        fwd.Normalize();

        Vector3 pos = o.position + fwd * spawnForward + Vector3.up * spawnUp;
        Quaternion rot = Quaternion.LookRotation(fwd, Vector3.up);
        GetComponent<PlayerSkillCastAnimator>()?.PlayCast();

        PhotonNetwork.Instantiate(freezeProjectilePrefab, pos, rot);
    }
}
