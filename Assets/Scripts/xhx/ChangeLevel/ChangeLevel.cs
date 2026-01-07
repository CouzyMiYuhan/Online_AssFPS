using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 用于进度条
using System.Collections;
using Photon.Pun;

// 挂载到场景中常驻的 GameObject（如 GameManager）
public class ChangeLevel : MonoBehaviour
{
    public string nextLevelName;

    public static ChangeLevel Instance { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        PhotonView pv = other.GetComponent<PhotonView>();
        if (player != null && pv != null && pv.IsMine)
        {
            PhotonNetwork.LoadLevel(nextLevelName);
        }
    }

    //// 同步加载场景（简单场景使用）
    //public void LoadSceneSync(string sceneName)
    //{
    //    Time.timeScale = 1f; // 确保时间正常流动
    //    SceneManager.LoadScene(sceneName);
    //}

    //// 异步加载场景（推荐）
    //public void LoadSceneAsync(string sceneName)
    //{
    //    loadingPanel.ShowPanel();
    //    StartCoroutine(LoadSceneAsyncRoutine(sceneName));
    //}

    //// 核心异步加载协程
    //private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    //{
    //    // 2. 重置时间缩放
    //    Time.timeScale = 1f;

    //    // 3. 开始异步加载
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
    //    asyncLoad.allowSceneActivation = false; // 等待进度完成再激活

    //    // 4. 更新进度条
    //    while (!asyncLoad.isDone)
    //    {
    //        float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
    //        loadingPanel.SetProgress(progress); // 更新进度

    //        if (asyncLoad.progress >= 0.9f)
    //        {
    //            loadingPanel.SetProgress(1f);
    //            asyncLoad.allowSceneActivation = true;
    //        }
    //        yield return null;
    //    }

    //    // 6. 场景激活后隐藏加载界面
    //    loadingPanel.HidePanel();
    //}
}