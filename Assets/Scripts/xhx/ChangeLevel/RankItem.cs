using UnityEngine;
using UnityEngine.UI;

public class RankItem : MonoBehaviour
{
    public Text rankText;
    public Text playerNameText;
    public Text finishTimeText;
    public Image background;
    public Color firstPlaceColor = new Color(1f, 0.84f, 0f, 0.3f); // 金色
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f, 0.3f); // 银色
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f, 0.3f); // 铜色
    public Color defaultColor = new Color(0.2f, 0.2f, 0.2f, 0.3f); // 深灰

    public void Setup(int rank, string playerName, float finishTime, bool isLocalPlayer = false)
    {
        rankText.text = rank.ToString();
        playerNameText.text = playerName;
        finishTimeText.text = FormatTime(finishTime);

        // 根据排名设置背景色
        if (rank == 1) background.color = firstPlaceColor;
        else if (rank == 2) background.color = secondPlaceColor;
        else if (rank == 3) background.color = thirdPlaceColor;
        else background.color = defaultColor;

        // 如果是本地玩家，加粗显示
        if (isLocalPlayer)
        {
            playerNameText.fontStyle = FontStyle.Bold;
            playerNameText.color = new Color(0.2f, 0.8f, 1f); // 蓝色
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        float milliseconds = (time - (int)time) * 1000f;
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
}