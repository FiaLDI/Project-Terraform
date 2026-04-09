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
        public bool Crouch;
        public bool Grounded;
        public int WeaponPose;
    }

    private readonly List<Snapshot> snapshots = new();

    private const float InterpDelay = 0.05f;
    private const float TeleportThreshold = 5f;

    private PlayerAnimationController anim;

    private void Awake()
    {
        anim = GetComponentInParent<PlayerAnimationController>();
    }

    public void ReceiveState(PlayerState state)
    {
        float time = Time.time;

        if (snapshots.Count > 0)
        {
            var last = snapshots[snapshots.Count - 1];

            if (Vector3.Distance(last.Position, state.Position) > TeleportThreshold)
            {
                snapshots.Clear();
            }
        }

        snapshots.Add(new Snapshot
        {
            Time = time,
            Position = state.Position,
            Velocity = state.Velocity,
            Yaw = state.Yaw,
            Pitch = state.Pitch,
            Crouch = state.Crouch,
            Grounded = state.Grounded,
            WeaponPose = state.WeaponPose
        });

        if (snapshots.Count > 32)
            snapshots.RemoveAt(0);
    }

    private void Update()
    {
        var netObj = GetComponentInParent<NetworkObject>();
        if (netObj != null && netObj.IsOwner)
            return;
        
        if (snapshots.Count < 2)
            return;

        float renderTime = snapshots[snapshots.Count - 1].Time - InterpDelay;

        while (snapshots.Count >= 2 && snapshots[1].Time <= renderTime)
        {
            snapshots.RemoveAt(0);
        }

        if (snapshots.Count < 2)
            return;

        var from = snapshots[0];
        var to   = snapshots[1];

        float t = Mathf.InverseLerp(from.Time, to.Time, renderTime);
        t = Mathf.Clamp01(t);

        Vector3 pos = Vector3.Lerp(from.Position, to.Position, t);

        if ((pos - transform.position).sqrMagnitude > TeleportThreshold * TeleportThreshold)
        {
            transform.position = pos;
        }
        else
        {
            transform.position = pos;
        }

        // ================= ROTATION =================

        float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        float pitch = Mathf.Lerp(from.Pitch, to.Pitch, t);
        var head = GetComponentInChildren<HeadPitchController>();
        head?.SetRemotePitch(pitch);

        // ================= ANIMATION =================

        if (anim != null)
        {
            Vector3 vel = Vector3.Lerp(from.Velocity, to.Velocity, t);

            float speed = new Vector2(vel.x, vel.z).magnitude;

            anim.SetSpeed(speed);
            anim.SetGrounded(to.Grounded);
            anim.SetCrouch(to.Crouch);
            anim.SetWeaponPose(to.WeaponPose);
        }
    }
}
