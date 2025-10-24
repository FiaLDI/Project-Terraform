using UnityEngine;

public class HeadFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform cameraTransform;

    private void LateUpdate()
    {
        if (!head || !cameraTransform) return;
        head.rotation = cameraTransform.rotation;
    }
}
