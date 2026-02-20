using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;
using Features.Player.UnityIntegration;

[RequireComponent(typeof(DeterministicMovement))]
public class PlayerNetworkController : NetworkBehaviour
{
    private DeterministicMovement movement;
    private MovementInputHandler inputHandler;

    private readonly Dictionary<int, MoveCommand> inputBuffer = new();
    private readonly Dictionary<int, PlayerState> stateBuffer = new();

    private const float IgnoreErrorThreshold = 0.05f;
    private const float HardSnapThreshold = 2f;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
    }

    public void InjectInput(MovementInputHandler handler)
    {
        inputHandler = handler;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        NetworkTickSystem.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        NetworkTickSystem.OnTick -= OnTick;
        inputBuffer.Clear();
        stateBuffer.Clear();
    }

    private void OnTick()
    {
        if (!IsSpawned)
            return;

        int currentTick = NetworkTickSystem.I.CurrentTick;

        // ================= OWNER SEND INPUT =================
        if (IsOwner)
        {
            if (inputHandler == null)
                return;

            var input = inputHandler.CurrentState;

            MoveCommand cmd = new MoveCommand
            {
                Tick   = currentTick,
                Move   = input.Move,
                Yaw    = input.Yaw,
                Jump   = input.Jump,
                Crouch = input.Crouch
            };

            inputBuffer[currentTick] = cmd;
            SendInputServerRpc(cmd);

            inputHandler.ClearOneShotFlags();
        }

        // ================= SERVER SIMULATION =================
        if (IsServer)
        {
            int simulationTick = currentTick - 1;
            if (simulationTick < 0)
                return;

            if (!inputBuffer.TryGetValue(simulationTick, out var cmd))
            {
                // используем последний input
                if (inputBuffer.Count == 0)
                    cmd = default;
                else
                {
                    int latestTick = -1;
                    foreach (var kvp in inputBuffer)
                        if (kvp.Key > latestTick)
                            latestTick = kvp.Key;

                    cmd = inputBuffer[latestTick];
                }
            }

            movement.Simulate(cmd);

            PlayerState state = new PlayerState
            {
                Tick     = simulationTick,
                Position = transform.position,
                Velocity = movement.Velocity,
                Yaw      = transform.eulerAngles.y
            };

            SendStateObserversRpc(state);
            SendStateTargetRpc(Owner, state);
        }
    }

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        inputBuffer[cmd.Tick] = cmd;
    }

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(PlayerState state)
    {
        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    [TargetRpc]
    private void SendStateTargetRpc(NetworkConnection conn, PlayerState serverState)
    {
        if (!stateBuffer.TryGetValue(serverState.Tick, out var predicted))
            return;

        float error = Vector3.Distance(predicted.Position, serverState.Position);

        if (error < IgnoreErrorThreshold)
            return;

        if (error > HardSnapThreshold)
        {
            transform.position = serverState.Position;
            return;
        }

        Vector3 delta = serverState.Position - transform.position;
        transform.position += delta * 0.5f;
    }
}
