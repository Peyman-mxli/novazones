using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerFrameUI : MonoBehaviour
{
    public enum PlayerRank
    {
        Player,
        VIP,
        GM,
        Admin,
        Enemy
    }

    [Header("References")]
    public PlayerHealth playerHealth;
    public PlayerMana playerMana;
    public PlayerStats playerStats;

    [Header("UI")]
    public TMP_Text playerNameText;
    public Image playerPortraitImage;
    public Slider healthSlider;
    public Slider manaSlider;
    public TMP_Text healthValueText;
    public TMP_Text manaValueText;
    public TMP_Text levelText;
    public TMP_Text rankText;

    [Header("Show / Hide")]
    public KeyCode togglePlayerFrameKey = KeyCode.Y;
    public float battleShowTime = 5f;

    [Header("Rank")]
    public PlayerRank currentRank = PlayerRank.Player;

    [Header("Gear Score UI")]
    public TMP_Text gearScoreText;
    public int currentGearScore = 0;

    private CanvasGroup canvasGroup;
    private int lastHealth = -1;
    private float battleTimer = 0f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        FindPlayerReferences();

        if (playerHealth != null)
            lastHealth = playerHealth.currentHealth;

        ShowFrame();
        UpdateUI();
    }

    void Update()
    {
        if (playerHealth == null || playerMana == null || playerStats == null)
            FindPlayerReferences();

        CheckToggleKey();
        CheckBattleAutoShow();

        UpdateUI();
    }

    void CheckToggleKey()
    {
        if (Input.GetKeyDown(togglePlayerFrameKey))
        {
            if (IsFrameVisible())
                HideFrame();
            else
                ShowFrame();
        }
    }

    void CheckBattleAutoShow()
    {
        if (playerHealth == null)
            return;

        if (lastHealth < 0)
            lastHealth = playerHealth.currentHealth;

        if (playerHealth.currentHealth < lastHealth)
        {
            battleTimer = battleShowTime;
            ShowFrame();
        }

        lastHealth = playerHealth.currentHealth;

        if (battleTimer > 0f)
        {
            battleTimer -= Time.deltaTime;
            ShowFrame();
        }
    }

    public void HideFrame()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowFrame()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void ToggleFrame()
    {
        if (IsFrameVisible())
            HideFrame();
        else
            ShowFrame();
    }

    public bool IsFrameVisible()
    {
        if (canvasGroup == null)
            return true;

        return canvasGroup.alpha > 0.5f;
    }

    void FindPlayerReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        if (playerHealth == null)
            playerHealth = playerObject.GetComponent<PlayerHealth>();

        if (playerMana == null)
            playerMana = playerObject.GetComponent<PlayerMana>();

        if (playerStats == null)
            playerStats = playerObject.GetComponent<PlayerStats>();
    }

    void UpdateUI()
    {
        if (playerHealth == null)
            return;

        if (playerNameText != null)
            playerNameText.text = playerHealth.playerName;

        if (playerPortraitImage != null && playerHealth.playerPortrait != null)
            playerPortraitImage.sprite = playerHealth.playerPortrait;

        UpdateHeroLevel();
        UpdateRank();
        UpdateHealth();
        UpdateMana();
        UpdateGearScore();
    }

    void UpdateHealth()
    {
        int healthPercent = 0;

        if (playerHealth.maxHealth > 0)
            healthPercent = Mathf.RoundToInt((float)playerHealth.currentHealth / playerHealth.maxHealth * 100f);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 100;
            healthSlider.value = healthPercent;
        }

        if (healthValueText != null)
        {
            if (playerHealth.currentHealth <= 0 || playerHealth.IsDead())
            {
                healthValueText.text = "Dead";
                healthValueText.color = HexToColor("#FF3333");
            }
            else
            {
                healthValueText.text = playerHealth.currentHealth + "/" + playerHealth.maxHealth + " (" + healthPercent + "%)";
                healthValueText.color = GetHealthColor(healthPercent);
            }
        }
    }

    Color GetHealthColor(int healthPercent)
    {
        if (healthPercent >= 70) return HexToColor("#33CC66");
        if (healthPercent >= 40) return HexToColor("#FFD700");
        if (healthPercent >= 1) return HexToColor("#FF3333");

        return HexToColor("#FF3333");
    }

    void UpdateMana()
    {
        if (playerMana == null)
            return;

        int manaPercent = 0;

        if (playerMana.maxMana > 0)
            manaPercent = Mathf.RoundToInt((float)playerMana.currentMana / playerMana.maxMana * 100f);

        if (manaSlider != null)
        {
            manaSlider.gameObject.SetActive(true);
            manaSlider.minValue = 0;
            manaSlider.maxValue = 100;
            manaSlider.value = manaPercent;
        }

        if (manaValueText != null)
        {
            manaValueText.gameObject.SetActive(true);
            manaValueText.text = playerMana.currentMana + "/" + playerMana.maxMana + " (" + manaPercent + "%)";
        }
    }

    void UpdateHeroLevel()
    {
        if (levelText == null)
            return;

        int currentLevel = 1;

        if (playerStats != null)
            currentLevel = playerStats.GetCurrentLevel();

        levelText.text = currentLevel.ToString();
        levelText.color = GetHeroLevelColor(currentLevel);
    }

    void UpdateRank()
    {
        if (rankText == null)
            return;

        rankText.text = currentRank.ToString().ToUpper();
        rankText.color = GetRankColor(currentRank);
    }

    Color GetRankColor(PlayerRank rank)
    {
        if (rank == PlayerRank.Player) return HexToColor("#3399FF");
        if (rank == PlayerRank.VIP) return HexToColor("#FFD700");
        if (rank == PlayerRank.GM) return HexToColor("#FF8800");
        if (rank == PlayerRank.Enemy) return HexToColor("#A64DFF");
        if (rank == PlayerRank.Admin) return HexToColor("#FF3333");

        return Color.white;
    }

    Color GetHeroLevelColor(int level)
    {
        if (level <= 9) return HexToColor("#FFFFFF");
        if (level <= 49) return HexToColor("#33CC66");
        if (level <= 99) return HexToColor("#3399FF");
        if (level <= 499) return HexToColor("#FFD700");
        if (level <= 4999) return HexToColor("#A64DFF");
        if (level <= 9999) return HexToColor("#FF8800");

        return HexToColor("#FF3333");
    }

    void UpdateGearScore()
    {
        if (gearScoreText == null)
            return;

        gearScoreText.text = "Gear Score : " + currentGearScore;
        gearScoreText.color = GetGearScoreColor(currentGearScore);
    }

    Color GetGearScoreColor(int score)
    {
        if (score <= 500) return HexToColor("#9E9E9E");
        if (score <= 2000) return HexToColor("#33CC66");
        if (score <= 5000) return HexToColor("#3399FF");
        if (score <= 10000) return HexToColor("#FFD700");
        if (score <= 50000) return HexToColor("#A64DFF");
        if (score <= 100000) return HexToColor("#FF8800");

        return HexToColor("#FF3333");
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}