using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ChatManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject chatPanel;
    public InputField chatInputField;
    public Button sendButton;
    public ScrollRect chatScrollRect;
    public Transform chatContent;
    public GameObject messagePrefab; // 需要包含 Text 组件的预制体

    [Header("Chat Settings")]
    public KeyCode toggleChatKey = KeyCode.Tab;
    public Color localPlayerMessageColor = new Color32(69, 229, 83, 255); // 亮绿色
    public Color otherPlayerMessageColor = new Color32(240, 240, 240, 255); // 浅灰色
    public Color systemMessageColor = new Color32(98, 174, 241, 255); // 蓝色
    public float messageDuration = 15f; // 消息在聊天窗口中显示的时间
    public int maxMessages = 50; // 最大消息数量
    public bool use24HourFormat = true;
    public bool enableDebugLogs = true;

    [Header("Notification Settings")]
    public GameObject notificationPanel;
    public Text notificationText;
    public float notificationDuration = 3f;

    private bool isChatPanelVisible = false;
    private Queue<GameObject> activeMessages = new Queue<GameObject>();
    private bool isInitialized = false;
    private PhotonView photonViewInstance;

    #region Singleton Pattern
    public static ChatManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 确保获取 PhotonView
        photonViewInstance = GetComponent<PhotonView>();
        if (photonViewInstance == null)
        {
            photonViewInstance = gameObject.AddComponent<PhotonView>();
            if (enableDebugLogs) Debug.Log("Added missing PhotonView component to ChatManager");
        }
    }
    #endregion

    private void Start()
    {
        InitializeChat();
    }

    private void Update()
    {
        HandleChatToggle();

        // 调试：检查 PhotonView 状态
        if (enableDebugLogs && Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"PhotonView IsValid: {photonViewInstance.isActiveAndEnabled}, IsMine: {photonViewInstance.IsMine}, ViewID: {photonViewInstance.ViewID}");
            Debug.Log($"IsConnectedAndReady: {PhotonNetwork.IsConnectedAndReady}");
        }
    }

    private void InitializeChat()
    {
        if (isInitialized) return;

        // 确保 PhotonView 存在
        if (photonViewInstance == null)
        {
            photonViewInstance = GetComponent<PhotonView>();
            if (photonViewInstance == null)
            {
                photonViewInstance = gameObject.AddComponent<PhotonView>();
                if (enableDebugLogs) Debug.Log("Added PhotonView during initialization");
            }
        }

        // 设置按钮事件
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners(); // 确保移除旧的监听器
            sendButton.onClick.AddListener(SendMessage);
            if (enableDebugLogs) Debug.Log("Send button listener set");
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("Send button reference is missing in ChatManager");
        }

        // 设置输入框事件
        if (chatInputField != null)
        {
            chatInputField.onEndEdit.RemoveAllListeners(); // 确保移除旧的监听器
            chatInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            if (enableDebugLogs) Debug.Log("Input field listener set");
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("Chat input field reference is missing in ChatManager");
        }

        // 默认隐藏聊天面板
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
            isChatPanelVisible = false;
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("Chat panel reference is missing in ChatManager");
        }

        // 隐藏通知面板
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        isInitialized = true;

        if (enableDebugLogs) Debug.Log("Chat system initialized");
    }

    private void HandleChatToggle()
    {
        if (Input.GetKeyDown(toggleChatKey) && PhotonNetwork.IsConnectedAndReady)
        {
            ToggleChatPanel();
        }
    }

    public void ToggleChatPanel()
    {
        isChatPanelVisible = !isChatPanelVisible;

        if (chatPanel != null)
        {
            chatPanel.SetActive(isChatPanelVisible);
        }

        if (isChatPanelVisible && chatInputField != null)
        {
            StartCoroutine(FocusChatInput());
        }
    }

    private IEnumerator FocusChatInput()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // 确保UI已更新
        if (chatInputField != null && isChatPanelVisible)
        {
            chatInputField.ActivateInputField();
            chatInputField.Select();
            if (enableDebugLogs) Debug.Log("Chat input field focused");
        }
    }

    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(chatInputField?.text))
        {
            if (enableDebugLogs) Debug.LogWarning("Message is empty or whitespace");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            if (enableDebugLogs) Debug.LogWarning("Photon network is not connected or ready");
            ShowSystemMessage("You are not connected to the server", Color.red);
            return;
        }

        if (photonViewInstance == null)
        {
            if (enableDebugLogs) Debug.LogError("PhotonView is null, cannot send message");
            ShowSystemMessage("Chat system error: PhotonView missing", Color.red);
            return;
        }

        string message = chatInputField.text.Trim();
        chatInputField.text = string.Empty;

        if (enableDebugLogs) Debug.Log($"Sending message: '{message}' from {PhotonNetwork.NickName}");

        // 本地回显
        AddMessage(PhotonNetwork.LocalPlayer.NickName, message, true);

        // 通过 RPC 发送给所有玩家
        try
        {
            photonViewInstance.RPC(nameof(ReceiveMessage), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, message);
            if (enableDebugLogs) Debug.Log("RPC call successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send RPC: {ex.Message}");
            ShowSystemMessage("Failed to send message: " + ex.Message, Color.red);
        }
    }

    private void OnInputFieldEndEdit(string value)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrEmpty(value))
            {
                SendMessage();
            }
        }
    }

    [PunRPC]
    private void ReceiveMessage(string senderName, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            if (enableDebugLogs) Debug.LogWarning("Received empty message");
            return;
        }

        if (enableDebugLogs) Debug.Log($"Received message from {senderName}: '{message}'");

        // 本地玩家的消息已经在 SendMessage 中处理，不再重复添加
        if (senderName == PhotonNetwork.LocalPlayer.NickName)
        {
            if (enableDebugLogs) Debug.Log("Skipping duplicate local message");
            return;
        }

        AddMessage(senderName, message, false);
    }

    private string GetFormattedTimestamp()
    {
        string format = use24HourFormat ? "HH:mm:ss" : "h:mm:ss tt";
        return DateTime.Now.ToString(format);
    }

    private void AddMessage(string senderName, string message, bool isLocalPlayer)
    {
        if (string.IsNullOrEmpty(message))
        {
            if (enableDebugLogs) Debug.LogWarning("Attempting to add empty message");
            return;
        }

        // 创建消息对象
        if (messagePrefab == null || chatContent == null)
        {
            if (enableDebugLogs) Debug.LogError("Message prefab or chat content is null");
            return;
        }

        GameObject messageObj = Instantiate(messagePrefab, chatContent);
        Text messageText = messageObj.GetComponent<Text>();

        if (messageText == null)
        {
            if (enableDebugLogs) Debug.LogError("Message prefab is missing Text component");
            Destroy(messageObj);
            return;
        }

        // 设置消息内容 - 时间精确到秒
        string formattedMessage = $"[{GetFormattedTimestamp()}] {senderName}: {message}";
        messageText.text = formattedMessage;

        // 设置消息颜色
        messageText.color = isLocalPlayer ? localPlayerMessageColor : otherPlayerMessageColor;

        // 添加到活动消息队列
        activeMessages.Enqueue(messageObj);

        // 限制最大消息数量
        while (activeMessages.Count > maxMessages)
        {
            GameObject oldMessage = activeMessages.Dequeue();
            Destroy(oldMessage);
        }

        // 自动滚动到底部
        StartCoroutine(ScrollToBottom());

        // 显示通知（非本地消息）
        if (!isLocalPlayer && notificationText != null && notificationPanel != null)
        {
            notificationText.text = $"[{GetFormattedTimestamp()}] {senderName}: {message}";
            notificationPanel.SetActive(true);
            StartCoroutine(HideNotification());
        }
    }

    public void ShowSystemMessage(string message)
    {
        ShowSystemMessage(message, systemMessageColor);
    }

    public void ShowSystemMessage(string message, Color color)
    {
        if (string.IsNullOrEmpty(message))
        {
            if (enableDebugLogs) Debug.LogWarning("Attempting to show empty system message");
            return;
        }

        if (messagePrefab == null || chatContent == null)
        {
            if (enableDebugLogs) Debug.LogError("Cannot show system message: prefab or content missing");
            return;
        }

        // 创建系统消息对象
        GameObject messageObj = Instantiate(messagePrefab, chatContent);
        Text messageText = messageObj.GetComponent<Text>();

        if (messageText == null)
        {
            Destroy(messageObj);
            return;
        }

        // 设置系统消息内容 - 时间精确到秒
        string formattedMessage = $"[{GetFormattedTimestamp()}] {message}";
        messageText.text = formattedMessage;
        messageText.color = color;

        // 添加到队列
        activeMessages.Enqueue(messageObj);

        // 限制最大数量
        while (activeMessages.Count > maxMessages)
        {
            GameObject oldMessage = activeMessages.Dequeue();
            Destroy(oldMessage);
        }

        StartCoroutine(ScrollToBottom());
    }

    public void AddPlayerJoinedMessage(string playerName)
    {
        ShowSystemMessage($"{playerName} joined the room", systemMessageColor);
    }

    public void AddPlayerLeftMessage(string playerName)
    {
        ShowSystemMessage($"{playerName} left the room", Color.yellow);
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(notificationDuration);
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void ClearChat()
    {
        foreach (var messageObj in activeMessages)
        {
            Destroy(messageObj);
        }
        activeMessages.Clear();
    }

    #region Photon Callbacks
    public override void OnJoinedRoom()
    {
        ShowSystemMessage($"You joined room \"{PhotonNetwork.CurrentRoom.Name}\"", Color.green);
        if (enableDebugLogs) Debug.Log($"ChatManager: Joined room {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnLeftRoom()
    {
        ShowSystemMessage("You left the room", Color.red);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerJoinedMessage(newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        AddPlayerLeftMessage(otherPlayer.NickName);
    }

    public override void OnJoinedLobby()
    {
        ShowSystemMessage("Joined lobby", systemMessageColor);
    }

    public override void OnLeftLobby()
    {
        ShowSystemMessage("Left lobby", systemMessageColor);
    }

    public override void OnConnectedToMaster()
    {
        if (enableDebugLogs) Debug.Log("ChatManager: Connected to master server");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (enableDebugLogs) Debug.Log($"ChatManager: Disconnected from server. Cause: {cause}");
    }
    #endregion
}