using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatPanelAutoCreator : EditorWindow
{
    private string chatManagerName = "ChatManager";
    private bool useTMP = false;
    private bool createNotificationPanel = true;

    [MenuItem("GameObject/UI/Create Chat Panel", false, 0)]
    static void CreateChatPanelMenu()
    {
        GetWindow<ChatPanelAutoCreator>("Create Chat Panel");
    }

    private void OnGUI()
    {
        GUILayout.Label("Chat Panel Auto-Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        chatManagerName = EditorGUILayout.TextField("Chat Manager Name", chatManagerName);
        useTMP = EditorGUILayout.Toggle("Use TextMeshPro", useTMP);
        createNotificationPanel = EditorGUILayout.Toggle("Create Notification Panel", createNotificationPanel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Chat Panel"))
        {
            CreateChatPanel();
            Close();
        }
    }

    private void CreateChatPanel()
    {
        // 检查Canvas是否存在
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 设置Canvas缩放
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log("Created new Canvas");
        }

        // 检查EventSystem是否存在
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem");
        }

        // 创建ChatManager对象（如果不存在）
        GameObject chatManagerObj = GameObject.Find(chatManagerName);
        ChatManager chatManager = null;

        if (chatManagerObj == null)
        {
            chatManagerObj = new GameObject(chatManagerName);
            chatManager = chatManagerObj.AddComponent<ChatManager>();

            // 添加PhotonView
            if (chatManagerObj.GetComponent<Photon.Pun.PhotonView>() == null)
            {
                chatManagerObj.AddComponent<Photon.Pun.PhotonView>();
            }

            Debug.Log($"Created {chatManagerName} GameObject with ChatManager component");
        }
        else
        {
            chatManager = chatManagerObj.GetComponent<ChatManager>();
            if (chatManager == null)
            {
                chatManager = chatManagerObj.AddComponent<ChatManager>();
                Debug.Log($"Added ChatManager component to existing {chatManagerName} GameObject");
            }

            // 确保有PhotonView
            if (chatManagerObj.GetComponent<Photon.Pun.PhotonView>() == null)
            {
                chatManagerObj.AddComponent<Photon.Pun.PhotonView>();
            }
        }

        // 设置ChatManager属性默认值
        if (chatManager != null)
        {
            Undo.RecordObject(chatManager, "Configure ChatManager");
            chatManager.maxMessages = 50;
            chatManager.messageDuration = 15f;
            chatManager.notificationDuration = 3f;

            // 设置颜色
            chatManager.localPlayerMessageColor = new Color32(69, 229, 83, 255); // 亮绿色
            chatManager.otherPlayerMessageColor = Color.white;
            chatManager.systemMessageColor = new Color32(98, 174, 241, 255); // 蓝色

            EditorUtility.SetDirty(chatManager);
        }

        // 创建聊天面板
        GameObject chatPanel = CreateChatUI(canvas.transform, chatManager);

        // 保存场景
        if (EditorApplication.isPlaying == false)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        Selection.activeGameObject = chatPanel;
        Debug.Log("Chat Panel created successfully!");
    }

    private GameObject CreateChatUI(Transform canvasTransform, ChatManager chatManager)
    {
        // 1. 创建聊天主面板
        GameObject chatPanel = CreateUIElement("ChatPanel", canvasTransform);
        chatPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        chatPanel.SetActive(false); // 默认隐藏

        // 设置锚点 - 底部居中
        RectTransform panelRect = chatPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.0f);
        panelRect.anchorMax = new Vector2(0.5f, 0.0f);
        panelRect.pivot = new Vector2(0.5f, 0.0f);
        panelRect.anchoredPosition = new Vector2(0, 30);
        panelRect.sizeDelta = new Vector2(500, 300);

        // 2. 创建顶部标题
        GameObject titlePanel = CreateUIElement("TitlePanel", chatPanel.transform);
        titlePanel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        RectTransform titleRect = titlePanel.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0, 30);

        GameObject titleTextObj = CreateUIElement("TitleText", titlePanel.transform);

        if (useTMP && IsTextMeshProAvailable())
        {
            TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Chat";
            titleText.fontSize = 16;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
        }
        else
        {
            Text titleText = titleTextObj.AddComponent<Text>();
            titleText.text = "Chat";
            titleText.fontSize = 16;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
        }

        RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.sizeDelta = Vector2.zero;

        // 3. 创建关闭按钮
        GameObject closeButton = CreateButton("CloseButton", titlePanel.transform, "X");
        closeButton.GetComponent<Button>().onClick.AddListener(() => { });

        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 0.5f);
        closeRect.anchorMax = new Vector2(1, 0.5f);
        closeRect.pivot = new Vector2(1, 0.5f);
        closeRect.anchoredPosition = new Vector2(-5, 0);
        closeRect.sizeDelta = new Vector2(25, 25);

        // 4. 创建消息显示区域（Scroll View）
        GameObject scrollView = CreateUIElement("ScrollView", chatPanel.transform);
        scrollView.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1.0f);
        scrollView.AddComponent<Mask>();

        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(5, 35); // 距离顶部35（标题高度）
        scrollRect.offsetMax = new Vector2(-5, -35); // 距离底部35（输入框高度）

        // 5. 创建ScrollRect组件
        ScrollRect scrollRectComp = scrollView.AddComponent<ScrollRect>();
        scrollRectComp.horizontal = false;
        scrollRectComp.movementType = ScrollRect.MovementType.Clamped;
        scrollRectComp.inertia = true;
        scrollRectComp.decelerationRate = 0.1f;

        // 6. 创建内容区域
        GameObject content = CreateUIElement("Content", scrollView.transform);
        content.AddComponent<VerticalLayoutGroup>();
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 2;
        layout.childAlignment = TextAnchor.UpperLeft;

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.sizeDelta = Vector2.zero;
        contentRect.pivot = new Vector2(0, 1); // 从顶部开始

        // 将内容区域绑定到ScrollRect
        scrollRectComp.content = contentRect;

        // 7. 创建消息预制体
        GameObject messagePrefab = CreateUIElement("MessagePrefab", null); // 不放在任何父对象下，稍后会设置为资源
        messagePrefab.AddComponent<LayoutElement>().preferredHeight = 25;

        if (useTMP && IsTextMeshProAvailable())
        {
            TextMeshProUGUI messageText = messagePrefab.AddComponent<TextMeshProUGUI>();
            messageText.text = "Player: Sample message";
            messageText.fontSize = 14;
            messageText.enableWordWrapping = true;
            messageText.richText = true;
        }
        else
        {
            Text messageText = messagePrefab.AddComponent<Text>();
            messageText.text = "Player: Sample message";
            messageText.fontSize = 14;
            messageText.supportRichText = true;
        }

        // 将预制体设为资源
        string prefabPath = "Assets/ChatMessagePrefab.prefab";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        PrefabUtility.SaveAsPrefabAsset(messagePrefab, prefabPath);
        GameObject.DestroyImmediate(messagePrefab);

        // 8. 创建输入区域
        GameObject inputPanel = CreateUIElement("InputPanel", chatPanel.transform);
        inputPanel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        RectTransform inputRect = inputPanel.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(1, 0);
        inputRect.pivot = new Vector2(0.5f, 0);
        inputRect.anchoredPosition = Vector2.zero;
        inputRect.sizeDelta = new Vector2(-10, 30); // 距离左右各5像素

        // 9. 创建输入框
        GameObject inputFieldObj = CreateUIElement("ChatInputField", inputPanel.transform);
        InputField inputField = inputFieldObj.AddComponent<InputField>();

        GameObject inputTextObj = CreateUIElement("Text", inputFieldObj.transform);
        GameObject placeholderObj = CreateUIElement("Placeholder", inputFieldObj.transform);

        if (useTMP && IsTextMeshProAvailable())
        {
            // 使用TextMeshPro
            TMP_InputField tmpInputField = inputFieldObj.AddComponent<TMP_InputField>();
            DestroyImmediate(inputField); // 删除标准InputField

            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 14;
            inputText.color = Color.white;

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Type a message...";
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1.0f);

            tmpInputField.textComponent = inputText;
            tmpInputField.placeholder = placeholderText;
        }
        else
        {
            // 使用标准UI
            Text inputText = inputTextObj.AddComponent<Text>();
            inputText.color = Color.white;
            inputText.supportRichText = true;

            Text placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "Type a message...";
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1.0f);

            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
        }

        Image inputImage = inputFieldObj.AddComponent<Image>();
        inputImage.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        RectTransform inputFieldRect = inputFieldObj.GetComponent<RectTransform>();
        inputFieldRect.anchorMin = new Vector2(0, 0.5f);
        inputFieldRect.anchorMax = new Vector2(1, 0.5f);
        inputFieldRect.pivot = new Vector2(0.5f, 0.5f);
        inputFieldRect.anchoredPosition = new Vector2(-40, 0); // 留出发送按钮空间
        inputFieldRect.sizeDelta = new Vector2(-20, 25); // 距离左右各10像素

        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = new Vector2(5, 0);
        inputTextRect.offsetMax = new Vector2(-5, 0);

        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(5, 0);
        placeholderRect.offsetMax = new Vector2(-5, 0);

        // 10. 创建发送按钮
        GameObject sendButtonObj = CreateButton("SendButton", inputPanel.transform, "Send");
        Button sendButton = sendButtonObj.GetComponent<Button>();

        RectTransform sendButtonRect = sendButtonObj.GetComponent<RectTransform>();
        sendButtonRect.anchorMin = new Vector2(1, 0.5f);
        sendButtonRect.anchorMax = new Vector2(1, 0.5f);
        sendButtonRect.pivot = new Vector2(1, 0.5f);
        sendButtonRect.anchoredPosition = new Vector2(-5, 0);
        sendButtonRect.sizeDelta = new Vector2(70, 25);

        // 11. 创建通知面板（可选）
        GameObject notificationPanel = null;
        Text notificationText = null;

        if (createNotificationPanel)
        {
            notificationPanel = CreateUIElement("NotificationPanel", canvasTransform);
            notificationPanel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.8f, 0.9f);
            notificationPanel.SetActive(false);

            RectTransform notifRect = notificationPanel.GetComponent<RectTransform>();
            notifRect.anchorMin = new Vector2(0.5f, 1);
            notifRect.anchorMax = new Vector2(0.5f, 1);
            notifRect.pivot = new Vector2(0.5f, 1);
            notifRect.anchoredPosition = new Vector2(0, -10);
            notifRect.sizeDelta = new Vector2(400, 30);

            GameObject notifTextObj = CreateUIElement("NotificationText", notificationPanel.transform);
            notificationText = notifTextObj.AddComponent<Text>();
            notificationText.text = "New message received";
            notificationText.fontSize = 14;
            notificationText.alignment = TextAnchor.MiddleCenter;
            notificationText.color = Color.white;

            RectTransform notifTextRect = notifTextObj.GetComponent<RectTransform>();
            notifTextRect.anchorMin = Vector2.zero;
            notifTextRect.anchorMax = Vector2.one;
            notifTextRect.offsetMin = new Vector2(10, 0);
            notifTextRect.offsetMax = new Vector2(-10, 0);
        }

        // 12. 绑定所有引用到ChatManager
        Undo.RecordObject(chatManager, "Bind Chat UI References");

        chatManager.chatPanel = chatPanel;
        chatManager.chatInputField = inputFieldObj.GetComponent<InputField>();
        chatManager.sendButton = sendButton;
        chatManager.chatScrollRect = scrollRectComp;
        chatManager.chatContent = content.transform;
        chatManager.messagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (createNotificationPanel)
        {
            chatManager.notificationPanel = notificationPanel;
            chatManager.notificationText = notificationText;
        }

        // 绑定关闭按钮功能
        Button closeButtonComp = closeButton.GetComponent<Button>();
        closeButtonComp.onClick.RemoveAllListeners();
        closeButtonComp.onClick.AddListener(() => {
            if (chatManager != null)
            {
                chatManager.ToggleChatPanel();
            }
        });

        EditorUtility.SetDirty(chatManager);

        return chatPanel;
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.layer = LayerMask.NameToLayer("UI");

        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }

        RectTransform rect = obj.AddComponent<RectTransform>();

        if (parent != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.sizeDelta = new Vector2(160, 30);
        }

        return obj;
    }

    private GameObject CreateButton(string name, Transform parent, string buttonText)
    {
        GameObject buttonObj = CreateUIElement(name, parent);
        Button button = buttonObj.AddComponent<Button>();

        // 设置按钮背景
        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.7f, 1.0f);

        // 添加按钮文本
        GameObject textObj = CreateUIElement("Text", buttonObj.transform);

        if (useTMP && IsTextMeshProAvailable())
        {
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }
        else
        {
            Text text = textObj.AddComponent<Text>();
            text.text = buttonText;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
        }

        // 设置文本位置
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonObj;
    }

    private bool IsTextMeshProAvailable()
    {
#if TMP_PRESENT
        return true;
#else
        return false;
#endif
    }
}