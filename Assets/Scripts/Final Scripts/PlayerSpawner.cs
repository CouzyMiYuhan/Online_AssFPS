using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class PlayerSpawner : MonoBehaviour
{
    public const string PROP_ROLE = "role";

    [Header("4个角色Prefab名字（必须在 Resources 下，名字一致）")]
    public string[] playerPrefabNames = new string[4]
    {
        "PlayerNetworkPrefab_0",
        "PlayerNetworkPrefab_1",
        "PlayerNetworkPrefab_2",
        "PlayerNetworkPrefab_3",
    };

    [Header("Spawn Points（可不填，不填就(0,0,0)）")]
    public Transform[] spawnPoints;

    private void Start()
    {
        if (!PhotonNetwork.InRoom) return;

        int actor = PhotonNetwork.LocalPlayer.ActorNumber;

        // ✅ 优先读玩家已选角色
        int roleIndex = -1;
        if (PhotonNetwork.LocalPlayer.CustomProperties != null &&
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PROP_ROLE, out object v) &&
            v is int idx && idx >= 0 && idx < playerPrefabNames.Length)
        {
            roleIndex = idx;
        }
        else
        {
            // 兜底：没选就按actor分配
            roleIndex = (actor - 1) % playerPrefabNames.Length;

            PhotonNetwork.LocalPlayer.SetCustomProperties(
                new Hashtable { { PROP_ROLE, roleIndex } }
            );
        }

        string prefabName = playerPrefabNames[roleIndex];

        int spIndex = (spawnPoints != null && spawnPoints.Length > 0)
            ? (actor - 1) % spawnPoints.Length
            : -1;

        Vector3 pos = (spIndex >= 0) ? spawnPoints[spIndex].position : Vector3.zero;
        Quaternion rot = (spIndex >= 0) ? spawnPoints[spIndex].rotation : Quaternion.identity;

        PhotonNetwork.Instantiate(prefabName, pos, rot);
    }
}
