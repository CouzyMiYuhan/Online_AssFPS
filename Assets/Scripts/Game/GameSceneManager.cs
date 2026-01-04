using UnityEngine;
using Photon.Pun;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab（拖 Resources 里的那个）")]
    public GameObject playerPrefab;

    [Header("玩家出生点")]
    public Transform[] spawnPoints;

    void Start()
    {
        Debug.Log($"[GameSceneManager] Start. Connected={PhotonNetwork.IsConnected}, InRoom={PhotonNetwork.InRoom}");

        // ※ 在线并且已经在房间里 → 用网络方式生成
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            SpawnNetworkPlayer();
        }
        else
        {
            // 直接跑 GameScene 进行单机测试的情况
            Debug.LogWarning("[GameSceneManager] 不在房间内，使用本地测试玩家。");
            SpawnLocalTestPlayer();
        }
    }

    void SpawnNetworkPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[GameSceneManager] playerPrefab 没有在 Inspector 里绑定！");
            return;
        }

        Transform sp = GetRandomSpawnPoint();

        // 用 prefab.name 当 Photon 实例化名，前提：prefab 在 Resources 目录下
        string prefabName = playerPrefab.name;

        Debug.Log($"[GameSceneManager] SpawnNetworkPlayer -> {prefabName} at {sp.position}");
        PhotonNetwork.Instantiate(prefabName, sp.position, sp.rotation);
    }

    void SpawnLocalTestPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[GameSceneManager] playerPrefab 没有绑定，无法本地生成测试玩家");
            return;
        }

        Transform sp = GetRandomSpawnPoint();
        Debug.Log($"[GameSceneManager] SpawnLocalTestPlayer at {sp.position}");

        Instantiate(playerPrefab, sp.position, sp.rotation);
    }

    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        Debug.LogWarning("[GameSceneManager] 没有设置 spawnPoints，使用自己位置作为出生点");
        return this.transform;
    }
}
