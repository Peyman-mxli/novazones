using UnityEngine;

public class TalentSpecUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject tankPanel;
    public GameObject dpsPanel;

    public void OpenTankSpec()
    {
        tankPanel.SetActive(true);
        dpsPanel.SetActive(false);
    }

    public void OpenDPSSpec()
    {
        tankPanel.SetActive(false);
        dpsPanel.SetActive(true);
    }
}