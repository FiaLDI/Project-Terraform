// Features/Camera/Domain/PlayerCameraState.cs
namespace Features.Camera.Domain
{
    public enum CameraViewMode
    {
        FirstPerson,
        ThirdPerson
    }

    public class PlayerCameraState
    {
        public CameraViewMode Mode = CameraViewMode.FirstPerson;

        public float Pitch;
        public float Yaw;

        public float MinPitch;
        public float MaxPitch;

        // TPS distance
        public float TpsDistance;

        // Transition blend: 0 = FPS, 1 = TPS
        public float Blend;

        // is transitioning?
        public bool IsTransitioning;
        public float TransitionSpeed = 5f;
    }
}
