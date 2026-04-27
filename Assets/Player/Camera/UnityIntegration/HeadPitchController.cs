using UnityEngine;
using FishNet.Object;
using Features.Camera.UnityIntegration;

public class HeadPitchController : MonoBehaviour
{
    [SerializeField] private float maxUp = 35f;
    [SerializeField] private float maxDown = -45f;
    [SerializeField] private float pitchWeight = 0.6f;

    private NetworkObject netObj;
    private PlayerCameraController playerCamera;

    private float remotePitch;

    private void Awake()
    {
        netObj = GetComponentInParent<NetworkObject>();
        playerCamera = GetComponentInParent<PlayerCameraController>();
    }

    public void SetRemotePitch(float pitch)
    {
        remotePitch = pitch;
    }

    public void BindCamera(PlayerCameraController cameraController)
    {
        playerCamera = cameraController;
    }

    private void LateUpdate()
    {
        if (netObj == null)
            return;

        float pitch;

        if (netObj.IsOwner)
        {
            pitch = playerCamera != null
                ? playerCamera.CurrentPitch
                : CameraServiceProvider.Control.State.Pitch;
        }
        else
        {
            pitch = remotePitch;
        }

        pitch = Mathf.Clamp(pitch, maxDown, maxUp);
        pitch *= pitchWeight;

        Quaternion animRotation = transform.localRotation;
        Quaternion pitchOffset = Quaternion.Euler(pitch, 0f, 0f);

        transform.localRotation = animRotation * pitchOffset;
    }
}
