using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
public class LevelManager : MonoBehaviourPunCallbacks
{
    [Header("Countdown Settings")]
    public float countdownTime = 5f;

    [Header("UI References")]
    public Text countdownText;      // 居中显示的倒计时文本
    public Text gameTimeText;       // 右上角显示的游戏时间
    public GameObject leaderboardPanel; // 排行榜面板
    public Transform leaderboardContent; // 排行榜内容容器
    public GameObject leaderboardItemPrefab; // 排行榜项预制体
    public Text countdownToNextLevelText; // 下一关倒计时文本

    [Header("Level Settings")]
    public string nextLevelName = "Level2";
    public float timeBetweenLevels = 5f; // 所有玩家完成后等待的时间

    public float gameTime = 0f;
    public bool gameStarted = false;
    private bool isCountingDown = false;
    private bool gameCompleted = false;
    private PhotonView photonView;

    // 玩家完成记录
    private Dictionary<int, PlayerFinishData> finishedPlayers = new Dictionary<int, PlayerFinishData>();
    private int totalPlayers = 0;
    private Coroutine countdownCoroutine;
    private Coroutine gameTimeCoroutine;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        totalPlayers = PhotonNetwork.PlayerList.Length;

        //SetupUI();

        // 只有MasterClient负责启动倒计时
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("StartMatchCountdown", 1f);
        }
    }

    private void SetupUI()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (gameTimeText != null)
        {
            gameTimeText.gameObject.SetActive(false);
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        if (countdownToNextLevelText != null)
        {
            countdownToNextLevelText.gameObject.SetActive(false);
        }
    }

    private void StartMatchCountdown()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        if (players.Length == 0)
        {
            Debug.LogWarning("No players found. Retrying countdown in 0.5 seconds.");
            Invoke("StartMatchCountdown", 0.5f);
            return;
        }

        foreach (PlayerController player in players)
        {
            if (player != null)
            {
                PhotonView playerPhotonView = player.GetComponent<PhotonView>();
                if (playerPhotonView != null)
                {
                    playerPhotonView.RPC("DisableMovement", RpcTarget.All);
                }
            }
        }

        StartCountdown();
    }

    private void StartCountdown()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (isCountingDown) return;
        isCountingDown = true;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        photonView.RPC("RPC_StartCountdown", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartCountdown()
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        float timer = countdownTime;

        while (timer > 0)
        {
            int displayTime = Mathf.CeilToInt(timer);
            UpdateCountdownDisplay(displayTime);
            photonView.RPC("RPC_UpdateCountdown", RpcTarget.All, displayTime);

            yield return new WaitForSeconds(1f);
            timer--;
        }

        photonView.RPC("RPC_StartGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_UpdateCountdown(int time)
    {
        UpdateCountdownDisplay(time);
    }

    private void UpdateCountdownDisplay(int time)
    {
        Debug.Log($"倒计时: {time}秒");
        if (countdownText != null)
        {
            countdownText.text = time.ToString();
            countdownText.gameObject.SetActive(true);
        }
    }

    [PunRPC]
    private void RPC_StartGame()
    {
        StartGame();
    }

    private void StartGame()
    {
        gameStarted = true;
        isCountingDown = false;
        gameCompleted = false;
        finishedPlayers.Clear();

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (countdownToNextLevelText != null) countdownToNextLevelText.gameObject.SetActive(false);

        if (gameTimeText != null)
        {
            gameTimeText.gameObject.SetActive(true);
            gameTimeText.text = "00:00";
        }

        Debug.Log("游戏开始！允许玩家移动");

        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player != null)
            {
                PhotonView playerPhotonView = player.GetComponent<PhotonView>();
                if (playerPhotonView != null)
                {
                    playerPhotonView.RPC("EnableMovement", RpcTarget.All);
                }
            }
        }

        gameTime = 0f;
        if (gameTimeCoroutine != null) StopCoroutine(gameTimeCoroutine);
        gameTimeCoroutine = StartCoroutine(GameTimerCoroutine());
    }

    private IEnumerator GameTimerCoroutine()
    {
        while (gameStarted && !gameCompleted)
        {
            gameTime += Time.deltaTime;

            if (Time.frameCount % 6 == 0 && gameTimeText != null)
            {
                UpdateGameTimeDisplay(gameTime);
            }

            yield return null;
        }
    }

    private void UpdateGameTimeDisplay(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        string formattedTime = $"{minutes:00}:{seconds:00}";

        if (gameTimeText != null) gameTimeText.text = formattedTime;
    }

    [PunRPC]
    public void RPC_PlayerFinished(int playerId, float finishTime)
    {
        // 记录玩家完成数据
        if (!finishedPlayers.ContainsKey(playerId))
        {
            PlayerFinishData data = new PlayerFinishData
            {
                playerId = playerId,
                finishTime = finishTime,
                playerName = GetPlayerNameById(playerId)
            };

            finishedPlayers[playerId] = data;
            Debug.Log($"玩家 {data.playerName} 完成关卡，用时: {finishTime:F2}秒");

            // 更新排行榜
            UpdateLeaderboard();

            // 检查是否所有玩家都完成了
            if (finishedPlayers.Count >= totalPlayers)
            {
                AllPlayersFinished();
            }
        }
    }

    private string GetPlayerNameById(int playerId)
    {
        Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);
        if (player != null && player.CustomProperties.ContainsKey("playerName"))
        {
            return player.CustomProperties["playerName"].ToString();
        }
        return $"Player {playerId}";
    }

    private void UpdateLeaderboard()
    {
        if (leaderboardPanel == null || leaderboardContent == null || leaderboardItemPrefab == null)
            return;

        // 显示排行榜面板
        leaderboardPanel.SetActive(true);

        // 清除现有内容
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // 按完成时间排序
        var sortedPlayers = finishedPlayers.Values
            .OrderBy(p => p.finishTime)
            .ToList();

        // 创建排行榜项
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            PlayerFinishData playerData = sortedPlayers[i];
            GameObject item = Instantiate(leaderboardItemPrefab, leaderboardContent);

            // 设置排行榜项内容
            Text[] texts = item.GetComponentsInChildren<Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString(); // 排名
                texts[1].text = playerData.playerName; // 玩家名称
                texts[2].text = FormatTime(playerData.finishTime); // 完成时间
            }
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        float milliseconds = (time - (int)time) * 1000f;
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }

    private void AllPlayersFinished()
    {
        gameCompleted = true;
        gameStarted = false;

        if (gameTimeCoroutine != null) StopCoroutine(gameTimeCoroutine);

        Debug.Log("所有玩家都完成了关卡！");

        // 显示倒计时到下一关
        if (countdownToNextLevelText != null)
        {
            countdownToNextLevelText.gameObject.SetActive(true);
            countdownToNextLevelText.text = $"{timeBetweenLevels}";
        }

        // 开始倒计时到下一关
        StartCoroutine(CountdownToNextLevel());
    }

    private IEnumerator CountdownToNextLevel()
    {
        float timer = timeBetweenLevels;

        while (timer > 0)
        {
            if (countdownToNextLevelText != null)
            {
                countdownToNextLevelText.text = $"Next Level: {Mathf.Ceil(timer)}";
            }

            yield return new WaitForSeconds(1f);
            timer--;
        }

        // 加载下一关
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_LoadNextLevel", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_LoadNextLevel()
    {
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        Debug.Log($"加载下一关: {nextLevelName}");

        // 重置玩家状态
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player != null)
            {
                PhotonView playerPhotonView = player.GetComponent<PhotonView>();
                if (playerPhotonView != null && playerPhotonView.IsMine)
                {
                    playerPhotonView.RPC("ResetPlayerState", RpcTarget.All);
                }
            }
        }

        // 加载下一关
        PhotonNetwork.LoadLevel(nextLevelName);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        totalPlayers = PhotonNetwork.PlayerList.Length;
        SyncGameStateToPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        totalPlayers = PhotonNetwork.PlayerList.Length;

        // 如果玩家离开时游戏正在进行，检查是否所有剩余玩家都完成了
        if (gameStarted && finishedPlayers.Count >= totalPlayers)
        {
            AllPlayersFinished();
        }
    }

    private void SyncGameStateToPlayer(Photon.Realtime.Player player)
    {
        if (gameCompleted)
        {
            // 游戏已完成，同步排行榜和倒计时
            photonView.RPC("RPC_SyncCompletedState", player);
        }
        else if (gameStarted)
        {
            // 游戏进行中，同步游戏时间
            photonView.RPC("RPC_SyncGameTime", player, gameTime);
        }
        else if (isCountingDown)
        {
            // 倒计时中，同步倒计时状态
            float remainingTime = countdownTime;
            photonView.RPC("RPC_SyncCountdown", player, remainingTime);
        }
    }

    [PunRPC]
    private void RPC_SyncCompletedState()
    {
        // 同步已完成状态
        gameCompleted = true;
        gameStarted = false;

        // 显示排行榜
        UpdateLeaderboard();

        // 显示倒计时到下一关
        if (countdownToNextLevelText != null)
        {
            countdownToNextLevelText.gameObject.SetActive(true);
            countdownToNextLevelText.text = $"下一关: {timeBetweenLevels}";
        }

        // 开始倒计时（只在MasterClient执行）
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CountdownToNextLevel());
        }
    }

    [PunRPC]
    private void RPC_SyncGameTime(float time)
    {
        gameTime = time;
        if (!gameStarted && !isCountingDown && !gameCompleted)
        {
            StartGame();
        }
    }

    [PunRPC]
    private void RPC_SyncCountdown(float remainingTime)
    {
        if (isCountingDown) return;

        isCountingDown = true;
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);

        if (countdownText != null) countdownText.gameObject.SetActive(true);

        countdownCoroutine = StartCoroutine(ResumeCountdownCoroutine(remainingTime));
    }

    private IEnumerator ResumeCountdownCoroutine(float remainingTime)
    {
        float timer = remainingTime;

        while (timer > 0)
        {
            int displayTime = Mathf.CeilToInt(timer);
            UpdateCountdownDisplay(displayTime);
            photonView.RPC("RPC_UpdateCountdown", RpcTarget.All, displayTime);

            yield return new WaitForSeconds(1f);
            timer--;
        }

        RPC_StartGame();
    }

    void OnDestroy()
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        if (gameTimeCoroutine != null) StopCoroutine(gameTimeCoroutine);
    }

    // 玩家完成数据类
    [System.Serializable]
    public class PlayerFinishData
    {
        public int playerId;
        public string playerName;
        public float finishTime;
    }
}