using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;
using Features.Player.UnityIntegration;

public class PlayerNetworkController : NetworkBehaviour
{
    private DeterministicMovement movement;
    private MovementInputHandler inputHandler;

    private Dictionary<int, MoveCommand> inputBuffer = new();
    private Dictionary<int, PlayerState> stateBuffer = new();

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            inputHandler = FindObjectOfType<MovementInputHandler>();
            GetComponent<PlayerCameraController>()?.SetLocal(true);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || inputHandler == null)
            return;

        var input = inputHandler.CurrentState;

        int tick = NetworkTickSystem.I.CurrentTick;

        MoveCommand cmd = new MoveCommand
        {
            Tick = tick,
            Move = input.Move,
            Yaw  = input.Yaw,
            Jump = input.Jump
        };

        movement.Simulate(cmd);

        inputBuffer[tick] = cmd;

        stateBuffer[tick] = new PlayerState
        {
            Tick = tick,
            Position = transform.position,
            Velocity = movement.Velocity
        };

        SendInputServerRpc(cmd);

        inputHandler.ClearOneShotFlags();
    }

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        // ❗ Хост уже симулировал
        if (IsOwner && IsServer)
            return;

        movement.Simulate(cmd);

        PlayerState state = new PlayerState
        {
            Tick = cmd.Tick,
            Position = transform.position,
            Velocity = movement.Velocity,
            Yaw = cmd.Yaw
        };

        SendStateTargetRpc(Owner, state);
        SendStateObserversRpc(state);
    }

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (IsOwner)
            return;

        GetComponent<RemoteInterpolation>()?.ReceiveState(state);
    }

    [TargetRpc]
    private void SendStateTargetRpc(NetworkConnection conn, PlayerState serverState)
    {
        if (IsServer)
            return;

        if (!stateBuffer.TryGetValue(serverState.Tick, out var predicted))
            return;

        float error = Vector3.Distance(
            predicted.Position,
            serverState.Position);

        if (error > 0.05f)
        {
            transform.position = serverState.Position;
            movement.Velocity = serverState.Velocity;

            int currentTick = NetworkTickSystem.I.CurrentTick;

            for (int t = serverState.Tick + 1; t <= currentTick; t++)
            {
                if (inputBuffer.TryGetValue(t, out var cmd))
                    movement.Simulate(cmd);
            }
        }
    }

    public void InjectInput(MovementInputHandler handler)
    {
        inputHandler = handler;
    }

}
