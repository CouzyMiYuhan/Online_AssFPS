using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerHealthUI : MonoBehaviour
{
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    private CharacterHealth playerHealth;

    void Start()
    {
        // 联机模式：自动找到本地玩家的 CharacterHealth
        if (PhotonNetwork.IsConnected)
        {
            var allHealth = FindObjectsOfType<CharacterHealth>();
            foreach (var h in allHealth)
            {
                var view = h.GetComponent<PhotonView>();
                if (view != null && view.IsMine)
                {
                    playerHealth = h;
                    break;
                }
            }

            if (playerHealth == null)
            {
                Debug.LogError("[PlayerHealthUI] 找不到本地玩家的 CharacterHealth");
                return;
            }
        }
        else
        {
            // 单机模式：你可以在 Inspector 里手动拖，或者 FindObjectOfType
            playerHealth = FindObjectOfType<CharacterHealth>();
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.currentHealth;
        }

        UpdateUI(playerHealth.currentHealth, playerHealth.maxHealth);

        playerHealth.onHealthChanged += UpdateUI;
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged -= UpdateUI;
    }

    void UpdateUI(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"{current:0}/{max:0}";
        }
    }
}
