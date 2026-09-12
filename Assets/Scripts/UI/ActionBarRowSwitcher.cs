using UnityEngine;

public class ActionBarRowSwitcher : MonoBehaviour
{
    [Header("Rows")]
    public GameObject row01;
    public GameObject row02;
    public GameObject row03;

    [Header("Runtime")]
    public int currentRow = 1;

    private void Start()
    {
        ShowRow(currentRow);
    }

    public void ShowRow(int rowNumber)
    {
        if (rowNumber < 1)
            rowNumber = 3;

        if (rowNumber > 3)
            rowNumber = 1;

        currentRow = rowNumber;

        if (row01 != null)
            row01.SetActive(currentRow == 1);

        if (row02 != null)
            row02.SetActive(currentRow == 2);

        if (row03 != null)
            row03.SetActive(currentRow == 3);
    }

    public void NextRow()
    {
        ShowRow(currentRow + 1);
    }

    public void PreviousRow()
    {
        ShowRow(currentRow - 1);
    }
}