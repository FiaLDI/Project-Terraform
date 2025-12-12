using UnityEngine;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    public class HeadFollowAdapter : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform cameraTransform;

        private void LateUpdate()
        {
            if (!head || !cameraTransform) return;

            head.rotation = cameraTransform.rotation;
        }
    }
}
