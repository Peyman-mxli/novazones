using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Distance")]
    public float distance = 8f;
    public float minDistance = 3f;
    public float maxDistance = 15f;
    public float zoomSpeed = 2f;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minPitch = 10f;
    public float maxPitch = 70f;

    [Header("Height")]
    public float targetHeight = 1.5f;

    [Header("Smooth")]
    public float followSmoothSpeed = 10f;

    private float yaw = 0f;
    private float pitch = 25f;

    void LateUpdate()
    {
        if (target == null)
            return;

        bool mouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (!mouseOverUI && Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        if (!mouseOverUI)
        {
            float scroll = Input.mouseScrollDelta.y;
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed * Time.deltaTime);
        transform.LookAt(targetPosition);
    }
}