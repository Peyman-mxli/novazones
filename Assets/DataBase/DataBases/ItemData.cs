using UnityEngine;

public enum ItemType
{
    Money,
    Bag,
    Weapon,
    Armor,
    Material,
    Consumable,
    Quest,
    Misc
}

public enum ItemQuality
{
    // 🧱 EARLY GAME (1–200)
    Common,        // White      (1–10)
    Uncommon,      // Gray       (11–50)
    Worn,          // Silver     (51–100)
    Rare,          // Cyan       (101–150)
    Sarbaz,        // Blue       (151–200)

    // ⚔️ MID GAME (200–500)
    Enhanced,      // Lime       (201–250)
    Epic,          // Green      (251–300)
    Mega,          // Rose       (301–350)
    Mystic,        // Pink       (351–400)
    Ultra,         // Violet     (401–450)
    Master,        // Purple     (451–500)

    // 🔥 LATE GAME (500–800)
    Arcanic,       // Bright Yellow (501–550)
    Heroic,        // Yellow        (551–600)
    Ancient,       // Amber         (601–650)
    Legendary,     // Orange        (651–700)

    // ☠️ END GAME (800–1000)
    Rampage,       // Crimson    (701–800)
    Godlike,       // Red        (801–900)
    Sartip,        // Dark Red   (901–1000)

    // ⭐ SPECIAL
    VIP            // Golden (1–1000, special tier)
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Nova Zone/Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId = "item_001";
    public string itemName = "New Item";
    public Sprite itemIcon;

    [Header("Item Type")]
    public ItemType itemType = ItemType.Misc;
    public ItemQuality itemQuality = ItemQuality.Common;

    [Header("Item Level (1–1000)")]
    public int itemLevel = 1;

    [Header("Stacking")]
    public bool isStackable = true;
    public int maxStackSize = 25;

    [Header("Money")]
    public int goldValue = 0;
    public int silverValue = 0;

    [Header("Bag")]
    public int bagSlotCount = 0; // 0 = not a bag, 8/12/16/20/24 = bag

    [Header("Description")]
    [TextArea(3, 6)]
    public string itemDescription = "";
}