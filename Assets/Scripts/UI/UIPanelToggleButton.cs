using UnityEngine;

public class UIPanelToggleButton : MonoBehaviour
{
    [Header("Main Panel To Toggle")]
    public GameObject targetPanel;

    [Header("Panels To Close When Main Panel Closes")]
    public GameObject[] panelsToClose;

    public void TogglePanel()
    {
        if (targetPanel == null)
            return;

        bool newState = !targetPanel.activeSelf;
        targetPanel.SetActive(newState);

        if (newState == false)
            CloseExtraPanels();
    }

    void CloseExtraPanels()
    {
        for (int i = 0; i < panelsToClose.Length; i++)
        {
            if (panelsToClose[i] != null)
                panelsToClose[i].SetActive(false);
        }
    }
}