using UnityEngine;

public struct PlayerState
{
    public int Tick;
    public Vector3 Position;
    public Vector3 Velocity;
    public float Yaw;
    public float Pitch;
    public bool Jump;
    public bool Crouch;
    public bool Sprint;
    public bool Grounded;
}
