using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootWindowUI : MonoBehaviour
{
    public static LootWindowUI Instance;

    [Header("Main Window")]
    public GameObject lootWindow;

    [Header("Loot Buttons")]
    public Button[] lootButtons;

    [Header("Loot Texts")]
    public TMP_Text[] lootTexts;

    [Header("Loot Icons")]
    public Image[] lootIcons;

    [Header("Loot Counts")]
    public TMP_Text[] lootCountTexts;

    [Header("Floating Text (NO PREFAB)")]
    public FloatingLootText floatingText;

    [Header("Window Position")]
    public Vector2 openOffset = new Vector2(25f, -25f);

    [Header("Distance Close")]
    public Transform player;
    public float closeDistance = 4f;

    private EnemyLoot currentLoot;

    private void Awake()
    {
        Instance = this;

        if (lootWindow != null)
            lootWindow.SetActive(false);
    }

    private void Update()
    {
        if (lootWindow == null || !lootWindow.activeSelf)
            return;

        if (player == null || currentLoot == null)
            return;

        float distance = Vector3.Distance(player.position, currentLoot.transform.position);

        if (distance > closeDistance)
            CloseLootWindow();
    }

    public void OpenLootWindow(EnemyLoot enemyLoot)
    {
        if (enemyLoot == null)
            return;

        if (IsCtrlShiftHeld())
        {
            LootAll(enemyLoot);
            return;
        }

        currentLoot = enemyLoot;

        if (lootWindow != null)
        {
            lootWindow.SetActive(true);
            MoveLootWindowToMouse();
        }

        RefreshLootWindow();
    }

    private bool IsCtrlShiftHeld()
    {
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        return ctrlHeld && shiftHeld;
    }

    private void LootAll(EnemyLoot enemyLoot)
    {
        if (enemyLoot == null)
            return;

        int safetyLimit = 100;
        int index = 0;

        while (enemyLoot.HasAnyLoot() && safetyLimit > 0)
        {
            ItemData itemData = enemyLoot.GetLootItemData(index);
            int amount = enemyLoot.GetLootAmount(index);

            bool looted = enemyLoot.TakeItem(index);

            if (looted)
                ShowFloatingText(itemData, amount);

            index++;

            if (index >= 50)
                index = 0;

            safetyLimit--;
        }

        CloseLootWindow();
    }

    private void MoveLootWindowToMouse()
    {
        if (lootWindow == null)
            return;

        RectTransform lootRect = lootWindow.GetComponent<RectTransform>();
        RectTransform parentRect = lootWindow.transform.parent as RectTransform;

        if (lootRect == null || parentRect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            Input.mousePosition,
            null,
            out Vector2 localPoint
        );

        lootRect.anchoredPosition = localPoint + openOffset;
    }

    public void CloseLootWindow()
    {
        currentLoot = null;

        if (lootWindow != null)
            lootWindow.SetActive(false);
    }

    public void RefreshLootWindow()
    {
        if (lootButtons == null)
            return;

        for (int i = 0; i < lootButtons.Length; i++)
        {
            string displayText = currentLoot != null ? currentLoot.GetLootDisplayText(i) : "";
            ItemData itemData = currentLoot != null ? currentLoot.GetLootItemData(i) : null;
            int amount = currentLoot != null ? currentLoot.GetLootAmount(i) : 0;

            bool hasLoot = !string.IsNullOrEmpty(displayText) && itemData != null;

            if (lootButtons[i] != null)
                lootButtons[i].gameObject.SetActive(hasLoot);

            if (i < lootTexts.Length && lootTexts[i] != null)
            {
                lootTexts[i].text = hasLoot ? itemData.itemName : "";
                lootTexts[i].color = hasLoot ? GetQualityColor(itemData.itemQuality) : Color.white;
            }

            if (i < lootIcons.Length && lootIcons[i] != null)
            {
                lootIcons[i].enabled = hasLoot && itemData.itemIcon != null;
                lootIcons[i].sprite = hasLoot ? itemData.itemIcon : null;
            }

            if (i < lootCountTexts.Length && lootCountTexts[i] != null)
                lootCountTexts[i].text = (hasLoot && amount > 1) ? amount.ToString() : "";
        }
    }

    public void LootItemByIndex(int itemIndex)
    {
        if (currentLoot == null)
            return;

        ItemData itemData = currentLoot.GetLootItemData(itemIndex);
        int amount = currentLoot.GetLootAmount(itemIndex);

        bool looted = currentLoot.TakeItem(itemIndex);

        if (looted)
        {
            ShowFloatingText(itemData, amount);
            RefreshLootWindow();
        }

        if (!currentLoot.HasAnyLoot())
            CloseLootWindow();
    }

    private void ShowFloatingText(ItemData item, int amount)
    {
        if (floatingText == null || item == null)
            return;

        string message = amount > 1
            ? "+" + amount + " " + item.itemName
            : "+ " + item.itemName;

        floatingText.ShowText(message);
    }

    private Color GetQualityColor(ItemQuality quality)
    {
        switch (quality)
        {
            case ItemQuality.Common: return new Color32(255, 255, 255, 255);
            case ItemQuality.Uncommon: return new Color32(157, 157, 157, 255);
            case ItemQuality.Worn: return new Color32(192, 192, 192, 255);
            case ItemQuality.Rare: return new Color32(0, 255, 255, 255);
            case ItemQuality.Sarbaz: return new Color32(0, 112, 221, 255);
            case ItemQuality.Enhanced: return new Color32(191, 255, 0, 255);
            case ItemQuality.Epic: return new Color32(30, 255, 0, 255);
            case ItemQuality.Mega: return new Color32(255, 0, 127, 255);
            case ItemQuality.Mystic: return new Color32(255, 105, 180, 255);
            case ItemQuality.Ultra: return new Color32(138, 43, 226, 255);
            case ItemQuality.Master: return new Color32(163, 53, 238, 255);
            case ItemQuality.Arcanic: return new Color32(255, 255, 0, 255);
            case ItemQuality.Heroic: return new Color32(255, 215, 0, 255);
            case ItemQuality.Ancient: return new Color32(255, 191, 0, 255);
            case ItemQuality.Legendary: return new Color32(255, 128, 0, 255);
            case ItemQuality.Rampage: return new Color32(220, 20, 60, 255);
            case ItemQuality.Godlike: return new Color32(255, 0, 0, 255);
            case ItemQuality.Sartip: return new Color32(139, 0, 0, 255);
            case ItemQuality.VIP: return new Color32(255, 215, 0, 255);
            default: return Color.white;
        }
    }
}