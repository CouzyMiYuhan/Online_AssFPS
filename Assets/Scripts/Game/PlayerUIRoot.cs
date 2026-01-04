using UnityEngine;
using UnityEngine.UI;

public class PlayerUIRoot : MonoBehaviour
{
    public static PlayerUIRoot Instance { get; private set; }

    [Header("元素图标")]
    public Image elementIcon;   // 把 Canvas 里的 element Image 拖进来

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
