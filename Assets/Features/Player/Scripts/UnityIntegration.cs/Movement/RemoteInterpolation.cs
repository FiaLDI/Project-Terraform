using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;

public class RemoteInterpolation : NetworkBehaviour
{
    private struct Snapshot
    {
        public float Time;
        public Vector3 Position;
        public float Yaw;
    }

    private readonly List<Snapshot> snapshots = new();

    private const float InterpDelay = 0.1f;

    public void ReceiveState(PlayerState state)
    {
        float serverTime = state.Tick * NetworkTickSystem.TickDelta;

        snapshots.Add(new Snapshot
        {
            Time = serverTime,
            Position = state.Position,
            Yaw = state.Yaw
        });

        if (snapshots.Count > 32)
            snapshots.RemoveAt(0);
    }

    private void Update()
    {
        if (IsOwner)
            return;

        if (snapshots.Count < 2)
            return;

        float renderTime =
            NetworkTickSystem.I.CurrentTick *
            NetworkTickSystem.TickDelta -
            InterpDelay;

        while (snapshots.Count >= 2 &&
               snapshots[1].Time <= renderTime)
        {
            snapshots.RemoveAt(0);
        }

        if (snapshots.Count < 2)
            return;

        var from = snapshots[0];
        var to = snapshots[1];

        float t = Mathf.InverseLerp(from.Time, to.Time, renderTime);

        transform.position =
            Vector3.Lerp(from.Position, to.Position, t);

        float rot =
            Mathf.LerpAngle(from.Yaw, to.Yaw, t);

        transform.rotation =
            Quaternion.Euler(0f, rot, 0f);
    }
}
