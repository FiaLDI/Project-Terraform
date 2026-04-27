using FishNet.Object;
using UnityEngine;
using System;

public sealed class ServerGamePhase : NetworkBehaviour
{
    public GamePhase Current { get; private set; } = GamePhase.None;

    public event Action<GamePhase> OnPhaseReached;

    public void Reach(GamePhase phase)
    {
        if (!IsServer) return;
        if (phase <= Current) return;

        Current = phase;
        Debug.Log($"[GAME PHASE] → {phase}", this);
        OnPhaseReached?.Invoke(phase);
    }

    public bool IsAtLeast(GamePhase phase) => Current >= phase;
}
