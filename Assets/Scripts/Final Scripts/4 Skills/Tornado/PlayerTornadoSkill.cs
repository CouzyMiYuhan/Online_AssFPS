using Photon.Pun;
using UnityEngine;

public class PlayerTornadoSkill : MonoBehaviourPun
{
    [Header("Input")]
    public KeyCode castKey = KeyCode.E;

    [Header("Network Prefab (Resources/)")]
    public string tornadoPrefabName = "Skill_Tornado";

    [Header("Spawn")]
    public Transform castOrigin;          // 不填则用自己 transform
    public float spawnForward = 1.2f;
    public float spawnUp = 0.2f;

    [Header("Cooldown")]
    public float cooldown = 2.5f;

    private float _nextReadyTime = 0f;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(castKey) && Time.time >= _nextReadyTime)
        {
            _nextReadyTime = Time.time + cooldown;
            Cast();
        }
    }

    private void Cast()
    {
        Transform o = castOrigin != null ? castOrigin : transform;

        Vector3 pos = o.position + o.forward * spawnForward + Vector3.up * spawnUp;
        Quaternion rot = Quaternion.LookRotation(o.forward, Vector3.up);
        GetComponent<PlayerSkillCastAnimator>()?.PlayCast();
        PhotonNetwork.Instantiate(tornadoPrefabName, pos, rot);
    }
}
