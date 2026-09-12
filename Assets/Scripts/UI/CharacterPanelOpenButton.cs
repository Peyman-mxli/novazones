using UnityEngine;

public class CharacterPanelOpenButton : MonoBehaviour
{
    [Header("Window To Toggle")]
    public GameObject characterStatusWindow;

    public void ToggleCharacterWindow()
    {
        if (characterStatusWindow == null)
            return;

        characterStatusWindow.SetActive(!characterStatusWindow.activeSelf);
    }
}