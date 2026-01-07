using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class SkillCooldownHUD : MonoBehaviour
{
    public Image iconImage;
    public Image cooldownFill;
    public Text cooldownText;

    private ISkillCooldownReadable skill;

    private IEnumerator Start()
    {
        // 等本地玩家生成出来
        while (skill == null)
        {
            TryBindLocalSkill();
            yield return new WaitForSeconds(0.2f);
        }

        if (iconImage != null && skill.Icon != null)
            iconImage.sprite = skill.Icon;
    }

    private void TryBindLocalSkill()
    {
        // 找到本地玩家的 PhotonView
        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (!pv.IsMine) continue;

            // 在本地玩家身上找任何实现了 ISkillCooldownReadable 的脚本
            var monos = pv.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var m in monos)
            {
                if (m is ISkillCooldownReadable r)
                {
                    skill = r;
                    return;
                }
            }
        }
    }

    private void Update()
    {
        if (skill == null) return;

        float dur = Mathf.Max(0.01f, skill.CooldownDuration);
        float remain = Mathf.Max(0f, skill.CooldownRemaining);

        bool cooling = remain > 0.05f;

        // ✅ 图标
        if (iconImage != null && skill.Icon != null)
            iconImage.sprite = skill.Icon;

        // ✅ 遮罩：冷却中才显示，冷却好就隐藏
        if (cooldownFill != null)
        {
            cooldownFill.gameObject.SetActive(cooling);

            if (cooling)
                cooldownFill.fillAmount = Mathf.Clamp01(remain / dur); // 1->0 逐渐消失
        }

        // ✅ 数字：冷却中才显示
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(cooling);

            if (cooling)
                cooldownText.text = Mathf.CeilToInt(remain).ToString();
        }
    }

}
