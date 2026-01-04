using UnityEngine;
using Photon.Pun;

public class GameEndController : MonoBehaviourPunCallbacks
{
    [Header("Scene")]
    public string endSceneName = "EndScene";

    [Header("Player 查找")]
    public string playerLayerName = "Player";
    private int playerLayer;

    [Header("判定参数")]
    public float checkInterval = 1f;   // 每隔多久数一次人
    public int minPlayersToStart = 2;  // 至少多少人算“游戏正式开始”

    private float checkTimer;
    private bool gameStarted = false;  // 只有先达到过 minPlayersToStart，后面才会触发胜利

    void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
        if (playerLayer < 0)
        {
            Debug.LogError($"[PvP] 未找到 Layer '{playerLayerName}'，请在 Project Settings 里确认有这个 Layer。");
        }
    }

    void Start()
    {
        // 进场先重置一下结果，避免上一把残留
        GameResult.Reset();
    }

    void Update()
    {
        // 只有 MasterClient 负责统计和切场景
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient)
            return;

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = checkInterval;

        CheckAlivePlayers();
    }

    void CheckAlivePlayers()
    {
        var allHealth = FindObjectsOfType<CharacterHealth>();

        int aliveCount = 0;
        CharacterHealth lastAlive = null;

        foreach (var ch in allHealth)
        {
            // 只数 Player 层的角色
            if (ch.gameObject.layer != playerLayer)
                continue;

            if (ch.currentHealth > 0f)
            {
                aliveCount++;
                lastAlive = ch;
            }
        }

        Debug.Log($"[PvP] 当前 Player 层存活人数 = {aliveCount}");

        // 没有任何玩家时，不进行判定，等待玩家生成
        if (aliveCount == 0)
        {
            return;
        }

        // 第一次观察到人数达到 minPlayersToStart，算游戏正式开始
        if (!gameStarted && aliveCount >= minPlayersToStart)
        {
            gameStarted = true;
            Debug.Log("[PvP] 玩家人数已达开局要求，游戏开始判定生效。");
            return;
        }

        // 游戏尚未开始（例如刚进场只有 1 个玩家），不触发胜利
        if (!gameStarted)
            return;

        // 游戏开始之后，如果场上只剩 1 人 -> 胜利
        if (aliveCount == 1 && lastAlive != null)
        {
            int winnerActor = -1;
            var view = lastAlive.GetComponent<PhotonView>();
            if (view != null)
                winnerActor = view.OwnerActorNr;

            Debug.Log($"[PvP] 判定游戏结束，只剩一人存活，胜利方 Actor={winnerActor}");

            // 1）先用 RPC 把赢家广播给所有人，让每台机器自己算 IsVictory
            if (photonView != null)
            {
                photonView.RPC(nameof(RpcSetGameResult), RpcTarget.All, winnerActor);
            }
            else
            {
                Debug.LogError("[PvP] GameEndController 上缺少 PhotonView，无法同步胜负结果！");
            }

            // 2）再由 Master 调用 LoadLevel，同步切到结算场景
            PhotonNetwork.LoadLevel(endSceneName);
        }
    }

    /// <summary>
    /// 所有客户端都回调到这里，根据 winnerActor 算出自己是不是胜者
    /// </summary>
    [PunRPC]
    void RpcSetGameResult(int winnerActor)
    {
        int local = PhotonNetwork.LocalPlayer.ActorNumber;

        GameResult.WinnerActorNumber = winnerActor;
        GameResult.IsVictory = (winnerActor != -1 && local == winnerActor);

        Debug.Log($"[PvP] RpcSetGameResult -> winner={winnerActor}, local={local}, IsVictory={GameResult.IsVictory}");
    }

    // 现在不再在这里直接切场景，让 Master 人数判定来统一结束
    public void OnLocalPlayerDead()
    {
        Debug.Log("[PvP] 本地玩家死亡，等待 Master 判定游戏结束（不在这里切场景）");
    }
}
