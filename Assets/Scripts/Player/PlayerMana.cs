using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public int maxMana = 100;
    public int currentMana;

    void Start()
    {
        currentMana = maxMana;
    }

    public void UseMana(int amount)
    {
        if (amount <= 0)
            return;

        currentMana -= amount;

        if (currentMana < 0)
        {
            currentMana = 0;
        }

        Debug.Log("Player used " + amount + " mana. Current Mana: " + currentMana);
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0)
            return;

        currentMana += amount;

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
    }

    public void RestoreFullMana()
    {
        currentMana = maxMana;
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }

    public float GetManaPercent()
    {
        if (maxMana <= 0)
            return 0f;

        return (float)currentMana / maxMana;
    }
}