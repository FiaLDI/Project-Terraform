using System.Collections.Generic;
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

    private const float InterpDelay = 0.1f;
    private const float TeleportThreshold = 5f;

    private Transform rootTransform;
    private HeadPitchController head;
    private NetworkObject networkObject;

    private Vector3 interpolatedVelocity;
    private float interpolatedYaw;
    private bool interpolatedCrouch;
    private bool interpolatedGrounded;
    private int interpolatedWeaponPose;

    private void Awake()
    {
        networkObject = GetComponentInParent<NetworkObject>();
        rootTransform = networkObject != null ? networkObject.transform : transform;
        head = GetComponentInChildren<HeadPitchController>();
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

        if (snapshots.Count == 0)
        {
            ApplySnapshot(state.Position, state.Velocity, state.Yaw, state.Pitch, state.Crouch, state.Grounded, state.WeaponPose, snapPosition: true);
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
        if (networkObject != null && networkObject.IsOwner)
            return;

        if (snapshots.Count == 0)
            return;

        if (snapshots.Count == 1)
        {
            var snapshot = snapshots[0];
            ApplySnapshot(snapshot.Position, snapshot.Velocity, snapshot.Yaw, snapshot.Pitch, snapshot.Crouch, snapshot.Grounded, snapshot.WeaponPose, snapPosition: false);
            return;
        }

        float renderTime = snapshots[snapshots.Count - 1].Time - InterpDelay;

        while (snapshots.Count >= 2 && snapshots[1].Time <= renderTime)
        {
            snapshots.RemoveAt(0);
        }

        if (snapshots.Count == 1)
        {
            var snapshot = snapshots[0];
            ApplySnapshot(snapshot.Position, snapshot.Velocity, snapshot.Yaw, snapshot.Pitch, snapshot.Crouch, snapshot.Grounded, snapshot.WeaponPose, snapPosition: false);
            return;
        }

        var from = snapshots[0];
        var to   = snapshots[1];

        float t = Mathf.InverseLerp(from.Time, to.Time, renderTime);
        t = Mathf.Clamp01(t);

        Vector3 pos = Vector3.Lerp(from.Position, to.Position, t);
        float yaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
        float pitch = Mathf.Lerp(from.Pitch, to.Pitch, t);
        Vector3 velocity = Vector3.Lerp(from.Velocity, to.Velocity, t);
        bool grounded = t < 0.5f ? from.Grounded : to.Grounded;
        bool crouch = t < 0.5f ? from.Crouch : to.Crouch;
        int weaponPose = t < 0.5f ? from.WeaponPose : to.WeaponPose;

        ApplySnapshot(pos, velocity, yaw, pitch, crouch, grounded, weaponPose, snapPosition: false);
    }

    public float GetInterpolatedYaw()
    {
        return interpolatedYaw;
    }

    public Vector3 GetInterpolatedVelocity()
    {
        return interpolatedVelocity;
    }

    public bool IsGrounded()
    {
        return interpolatedGrounded;
    }

    public bool IsCrouching()
    {
        return interpolatedCrouch;
    }

    public int GetWeaponPose()
    {
        return interpolatedWeaponPose;
    }

    private void ApplySnapshot(
        Vector3 position,
        Vector3 velocity,
        float yaw,
        float pitch,
        bool crouch,
        bool grounded,
        int weaponPose,
        bool snapPosition)
    {
        interpolatedVelocity = velocity;
        interpolatedYaw = yaw;
        interpolatedCrouch = crouch;
        interpolatedGrounded = grounded;
        interpolatedWeaponPose = weaponPose;

        bool canDriveTransform = networkObject == null || !networkObject.IsServerStarted;

        if (canDriveTransform && rootTransform != null)
        {
            if (snapPosition || (position - rootTransform.position).sqrMagnitude > TeleportThreshold * TeleportThreshold)
            {
                rootTransform.position = position;
            }
            else
            {
                float smooth = 1f - Mathf.Exp(-15f * Time.deltaTime);
                rootTransform.position = Vector3.Lerp(rootTransform.position, position, smooth);
            }
        }

        head?.SetRemotePitch(pitch);
    }
}
