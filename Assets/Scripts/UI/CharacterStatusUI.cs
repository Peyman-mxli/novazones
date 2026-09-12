using UnityEngine;
using TMPro;

public class CharacterStatusUI : MonoBehaviour
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
    public PlayerStats playerStats;
    public PlayerFrameUI playerFrameUI;

    [Header("Character Info")]
    public string characterRaceOrClass = "Nova Pahlavan";

    [Header("Rank")]
    public PlayerRank currentRank = PlayerRank.Player;

    [Header("UI")]
    public TMP_Text characterInfoText;
    public TMP_Text rankText;
    public TMP_Text gearScoreText;

    void Start()
    {
        FindPlayerReferences();
        UpdateCharacterInfo();
        UpdateRank();
        UpdateGearScore();
    }

    void Update()
    {
        if (playerHealth == null || playerStats == null || playerFrameUI == null)
            FindPlayerReferences();

        UpdateCharacterInfo();
        UpdateRank();
        UpdateGearScore();
    }

    void FindPlayerReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            if (playerHealth == null)
                playerHealth = playerObject.GetComponent<PlayerHealth>();

            if (playerStats == null)
                playerStats = playerObject.GetComponent<PlayerStats>();
        }

        if (playerFrameUI == null)
            playerFrameUI = FindFirstObjectByType<PlayerFrameUI>();
    }

    void UpdateCharacterInfo()
    {
        if (characterInfoText == null || playerStats == null)
            return;

        int level = playerStats.GetCurrentLevel();
        characterInfoText.text = "Level " + level + " " + characterRaceOrClass;
    }

    void UpdateRank()
    {
        if (rankText == null)
            return;

        rankText.text = currentRank.ToString().ToUpper();
        rankText.color = GetRankColor(currentRank);
    }

    void UpdateGearScore()
    {
        if (gearScoreText == null)
            return;

        int gearScore = 0;

        if (playerFrameUI != null)
            gearScore = playerFrameUI.currentGearScore;

        gearScoreText.text = "GS: " + gearScore;
        gearScoreText.color = GetGearScoreColor(gearScore);
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