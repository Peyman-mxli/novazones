using UnityEngine;

public class BagInventoryTester : MonoBehaviour
{
    [Header("Test Bag Item")]
    public ItemData testBagItem;

    [Header("Target Bag Slot")]
    public int targetBagId = 1;

    [ContextMenu("Test Equip Bag")]
    public void TestEquipBag()
    {
        if (BagInventorySystem.Instance == null)
        {
            Debug.LogWarning("BagInventorySystem not found in scene.");
            return;
        }

        bool success = BagInventorySystem.Instance.TryEquipBagToSlot(testBagItem, targetBagId);

        if (success)
            Debug.Log("Bag equipped successfully.");
        else
            Debug.LogWarning("Bag equip failed.");
    }
}