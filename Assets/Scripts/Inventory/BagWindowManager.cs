using UnityEngine;

public class BagWindowManager : MonoBehaviour
{
    public static BagWindowManager Instance;

    [Header("Bag Windows")]
    public GameObject[] bagWindows; // assign in Inspector

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HideAllBags();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAllBags();
        }
    }

    public void ToggleBag(int bagId)
    {
        if (bagId < 0 || bagId >= bagWindows.Length)
            return;

        bool isActive = bagWindows[bagId].activeSelf;

        HideAllBags();

        if (!isActive)
        {
            bagWindows[bagId].SetActive(true);
        }
    }

    public void HideAllBags()
    {
        foreach (GameObject bag in bagWindows)
        {
            if (bag != null)
                bag.SetActive(false);
        }
    }

    public void CloseBag(GameObject bag)
    {
        if (bag != null)
            bag.SetActive(false);
    }
}