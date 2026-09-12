using TMPro;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text levelUpMessageText;

    [Header("Level Up Message")]
    public float levelUpMessageDuration = 2f;

    private float levelUpMessageTimer = 0f;

    void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateLevelUI;
            playerStats.OnLevelUp += ShowLevelUpMessage;
        }

        UpdateLevelUI();

        if (levelUpMessageText != null)
            levelUpMessageText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateLevelUI;
            playerStats.OnLevelUp -= ShowLevelUpMessage;
        }
    }

    void Update()
    {
        if (levelUpMessageText == null)
            return;

        if (!levelUpMessageText.gameObject.activeSelf)
            return;

        levelUpMessageTimer -= Time.deltaTime;

        if (levelUpMessageTimer <= 0f)
        {
            levelUpMessageText.gameObject.SetActive(false);
        }
    }

    void UpdateLevelUI()
    {
        if (playerStats == null)
            return;

        if (levelText != null)
        {
            int currentLevel = playerStats.GetCurrentLevel();
            levelText.text = "Level " + currentLevel;
            levelText.color = GetLevelColor(currentLevel);
        }
    }

    void ShowLevelUpMessage(int newLevel)
    {
        UpdateLevelUI();

        if (levelUpMessageText != null)
        {
            levelUpMessageText.text = "LEVEL UP! Level " + newLevel;
            levelUpMessageText.gameObject.SetActive(true);
            levelUpMessageTimer = levelUpMessageDuration;
        }
    }

    Color GetLevelColor(int level)
    {
        if (level <= 9) return HexToColor("#9E9E9E");
        if (level <= 49) return HexToColor("#33CC66");
        if (level <= 99) return HexToColor("#3399FF");
        if (level <= 499) return HexToColor("#FFD700");
        if (level <= 4999) return HexToColor("#A64DFF");
        if (level <= 9999) return HexToColor("#FF8800");

        return HexToColor("#FF3333");
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}