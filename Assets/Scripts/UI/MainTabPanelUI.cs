using UnityEngine;

public class MainTabPanelUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject characterTabPanel;
    public GameObject talentsTabPanel;
    public GameObject skillsTabPanel;

    public void OpenCharacterTab()
    {
        characterTabPanel.SetActive(true);
        talentsTabPanel.SetActive(false);
        skillsTabPanel.SetActive(false);
    }

    public void OpenTalentsTab()
    {
        characterTabPanel.SetActive(false);
        talentsTabPanel.SetActive(true);
        skillsTabPanel.SetActive(false);
    }

    public void OpenSkillsTab()
    {
        characterTabPanel.SetActive(false);
        talentsTabPanel.SetActive(false);
        skillsTabPanel.SetActive(true);
    }
}