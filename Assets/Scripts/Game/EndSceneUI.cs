using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndSceneUI : MonoBehaviour
{
    [Header("UI 根物体")]
    public GameObject victoryRoot;
    public GameObject defeatRoot;

    [Header("场景名字")]
    public string loginSceneName = "LobbyScene";

    [Header("按钮引用（用代码再绑一遍监听）")]
    public Button playAgainButton;
    public Button exitButton;

    private void Awake()
    {
        Debug.Log("[EndSceneUI] Awake in scene: " + SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        // ⭐ 只要进到结算场景，就强制释放鼠标
        UnlockCursor();
    }

    void UnlockCursor()
    {
        Time.timeScale = 1f;                   // 以防某处把时间停了
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[EndSceneUI] 解锁鼠标，显示光标");
    }

    private void Start()
    {
        bool victory = GameResult.IsVictory;

        if (victoryRoot != null)
            victoryRoot.SetActive(victory);

        if (defeatRoot != null)
            defeatRoot.SetActive(!victory);

        // ⭐ 用代码再绑定一遍按钮监听，顺便加 Debug
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(() =>
            {
                Debug.Log("[EndSceneUI] playAgainButton.onClick 被触发（来自代码绑定）");
                OnClickPlayAgain();
            });
        }
        else
        {
            Debug.LogWarning("[EndSceneUI] playAgainButton 没有在 Inspector 里赋值");
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() =>
            {
                Debug.Log("[EndSceneUI] exitButton.onClick 被触发（来自代码绑定）");
                OnClickExit();
            });
        }
        else
        {
            Debug.LogWarning("[EndSceneUI] exitButton 没有在 Inspector 里赋值");
        }
    }

    public void OnClickPlayAgain()
    {
        string current = SceneManager.GetActiveScene().name;
        Debug.Log($"[EndSceneUI] OnClickPlayAgain from '{current}', loginSceneName='{loginSceneName}'");

        // 保险：先检查是否可加载
        if (!Application.CanStreamedLevelBeLoaded(loginSceneName))
        {
            Debug.LogError(
                $"[EndSceneUI] 场景 '{loginSceneName}' 无法加载！" +
                " 请检查：1）名字是否和 Build Settings 中一致；2）是否已加入 Scenes In Build。");
            return;
        }

        // 重置 TimeScale（万一你有 Pause 逻辑）
        Time.timeScale = 1f;

        Debug.Log("[EndSceneUI] 调用 SceneManager.LoadScene ...");
        SceneManager.LoadScene(loginSceneName, LoadSceneMode.Single);
    }


    public void OnClickExit()
    {
        Debug.Log("[EndSceneUI] OnClickExit");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
