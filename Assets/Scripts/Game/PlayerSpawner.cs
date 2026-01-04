using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class PlayerSpawner : MonoBehaviour
{
    // 后面别的脚本/ UI 想知道玩家选了哪个角色，可以读这个 Key
    public const string PROP_ROLE = "role";

    [Header("4个角色Prefab名字（必须在 Assets/Resources/ 下，名字完全一致）")]
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

        // 进入顺序：1,2,3,4...
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;

        // 自动选角色（0~3）
        int roleIndex = (actor - 1) % playerPrefabNames.Length;
        string prefabName = playerPrefabNames[roleIndex];

        // 记录到玩家属性（方便后面做角色选择UI）
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable { { PROP_ROLE, roleIndex } }
        );

        // 用 actorNumber 决定出生点，避免随机导致两个人出生同一个点
        int spIndex = (spawnPoints != null && spawnPoints.Length > 0)
            ? (actor - 1) % spawnPoints.Length
            : -1;

        Vector3 pos = (spIndex >= 0) ? spawnPoints[spIndex].position : Vector3.zero;
        Quaternion rot = (spIndex >= 0) ? spawnPoints[spIndex].rotation : Quaternion.identity;

        PhotonNetwork.Instantiate(prefabName, pos, rot);
    }
}
