using Photon.Pun;
using UnityEngine;

public class PlayerTornadoSkill : MonoBehaviourPun, ISkillCooldownReadable
{
    [Header("Input")]
    public KeyCode castKey = KeyCode.E;

    [Header("UI")]
    public Sprite skillIcon;

    [Header("Network Prefab (Resources/)")]
    public string tornadoPrefabName = "Skill_Tornado";

    [Header("Spawn")]
    public Transform castOrigin;          // 不填则用自己 transform
    public float spawnForward = 1.2f;
    public float spawnUp = 0.2f;

    [Header("Cooldown")]
    public float cooldown = 2.5f;
    private float _nextReadyTime = 0f;

    // ====== ISkillCooldownReadable ======
    public float CooldownDuration => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, _nextReadyTime - Time.time);
    public bool IsReady => Time.time >= _nextReadyTime;
    public Sprite Icon => skillIcon;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(castKey) && IsReady)
        {
            _nextReadyTime = Time.time + cooldown;
            Cast();
        }
    }

    private void Cast()
    {
        Transform o = castOrigin != null ? castOrigin : transform;

        //  保持水平朝向（避免抬头导致生成飞天）
        Vector3 fwd = o.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;
        fwd.Normalize();

        Vector3 pos = o.position + fwd * spawnForward + Vector3.up * spawnUp;
        Quaternion rot = Quaternion.LookRotation(fwd, Vector3.up);

        GetComponent<PlayerSkillCastAnimator>()?.PlayCast();
        PhotonNetwork.Instantiate(tornadoPrefabName, pos, rot);
    }
}
