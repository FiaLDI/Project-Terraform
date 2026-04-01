using System.Collections.Generic;
using Features.Player.UnityIntegration;
using FishNet.Object;
using UnityEngine;

public class RemoteInterpolation : MonoBehaviour
{
    private struct Snapshot
    {
        public float Time;
        public Vector3 Position;
        public Vector3 Velocity;
        public float Yaw;
        public float Pitch;
        public bool Jump;
        public bool Crouch;
        public bool Sprint;
        public bool Grounded;
    }

    private readonly List<Snapshot> snapshots = new();
    private const float InterpDelay = 0.2f;
    private PlayerAnimationController anim;

    public void Awake()
    {
        anim = GetComponentInParent<PlayerAnimationController>();
    }

    public void ReceiveState(PlayerState state)
    {
        float time = Time.time;

        snapshots.Add(new Snapshot
        {
            Time = time,
            Position = state.Position,
            Velocity = state.Velocity,
            Yaw = state.Yaw,
            Pitch = state.Pitch,
            Jump = state.Jump,
            Crouch = state.Crouch,
            Sprint = state.Sprint,
            Grounded = state.Grounded
        });

        if (snapshots.Count > 20)
            snapshots.RemoveAt(0);
    }

    private void Update()
    {
        var netObj = GetComponentInParent<NetworkObject>();
        if (netObj != null && netObj.IsOwner)
            return;
            
        if (snapshots.Count < 2)
            return;

        float renderTime = Time.time - InterpDelay;

        while (snapshots.Count >= 2 &&
               snapshots[1].Time <= renderTime)
        {
            snapshots.RemoveAt(0);
        }

        if (snapshots.Count < 2)
            return;

        var from = snapshots[0];
        var to   = snapshots[1];

        float t = Mathf.InverseLerp(from.Time, to.Time, renderTime);

        transform.position = Vector3.Lerp(from.Position, to.Position, t);
        transform.rotation = Quaternion.Euler(
            0f,
            Mathf.LerpAngle(from.Yaw, to.Yaw, t),
            0f
        );
        float pitch = Mathf.Lerp(
            from.Pitch,
            to.Pitch,
            t
        );
        var head = GetComponentInChildren<HeadPitchController>();
        head?.SetRemotePitch(pitch);

        if (anim != null)
        {
            float speed = new Vector2(to.Velocity.x, to.Velocity.z).magnitude;
            anim.SetSpeed(speed);
            anim.SetGrounded(to.Grounded);
            anim.SetCrouch(to.Crouch);

            if (to.Jump)
                anim.TriggerJump();
        }
    }
}
