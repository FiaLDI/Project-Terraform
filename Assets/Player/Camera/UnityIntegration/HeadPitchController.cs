using UnityEngine;
using FishNet.Object;
using Features.Camera.UnityIntegration;

public class HeadPitchController : MonoBehaviour
{
    [SerializeField] private float maxUp = 35f;
    [SerializeField] private float maxDown = -45f;
    [SerializeField] private float pitchWeight = 0.6f;

    private NetworkObject netObj;

    private float remotePitch;

    private void Awake()
    {
        netObj = GetComponentInParent<NetworkObject>();
    }

    public void SetRemotePitch(float pitch)
    {
        remotePitch = pitch;
    }

    private void LateUpdate()
    {
        if (netObj == null)
            return;

        float pitch;

        if (netObj.IsOwner)
        {
            pitch = CameraServiceProvider.Control.State.Pitch;
        }
        else
        {
            pitch = remotePitch;
        }

        pitch = Mathf.Clamp(pitch, maxDown, maxUp);
        pitch *= pitchWeight;

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}