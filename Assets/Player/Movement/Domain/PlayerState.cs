using UnityEngine;

public struct PlayerState
{
    public int Tick;

    public Vector3 Position;
    public Vector3 Velocity;

    public float Yaw;
    public float Pitch;

    public float VerticalVelocity;
    public float InternalYaw;

    public bool Grounded;
    public bool Crouch;
    public int WeaponPose;
}
