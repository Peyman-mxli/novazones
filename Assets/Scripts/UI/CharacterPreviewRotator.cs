using UnityEngine;

public class CharacterPreviewRotator : MonoBehaviour
{
    [Header("Preview Model")]
    public Transform previewModel;

    [Header("Rotation")]
    public float rotateAmount = 25f;

    public void RotateLeft()
    {
        if (previewModel == null)
            return;

        previewModel.Rotate(0f, rotateAmount, 0f, Space.World);
    }

    public void RotateRight()
    {
        if (previewModel == null)
            return;

        previewModel.Rotate(0f, -rotateAmount, 0f, Space.World);
    }
}