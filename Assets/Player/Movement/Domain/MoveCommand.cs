using UnityEngine;

public struct MoveCommand
{
    public int Tick;
    public Vector2 Move;
    public float Yaw;
    public float Pitch;
    public bool Jump;
    public bool Sprint;
    public bool Crouch;
}
