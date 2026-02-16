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

    private const float HardCorrectionThreshold = 0.5f;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
    }

    public void InjectInput(MovementInputHandler handler)
    {
        inputHandler = handler;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        movement.IsServerAuthority = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
            GetComponent<PlayerCameraController>()?.SetLocal(true);

        NetworkTickSystem.OnTick += OnTick;
    }

    private void OnDestroy()
    {
        NetworkTickSystem.OnTick -= OnTick;
    }

    private void OnTick()
    {
        if (!IsOwner || inputHandler == null)
            return;

        int tick = NetworkTickSystem.I.CurrentTick;
        var input = inputHandler.CurrentState;

        MoveCommand cmd = new MoveCommand
        {
            Tick = tick,
            Move = input.Move,
            Yaw  = input.Yaw,
            Jump = input.Jump
        };

        // CLIENT prediction
        if (!IsServer)
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
        movement.Simulate(cmd);

        PlayerState state = new PlayerState
        {
            Tick = cmd.Tick,
            Position = transform.position,
            Velocity = movement.Velocity
        };

        SendStateTargetRpc(Owner, state);
        SendStateObserversRpc(state);
    }

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (IsOwner)
            return;

        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    [TargetRpc]
    private void SendStateTargetRpc(NetworkConnection conn, PlayerState serverState)
    {
        if (!stateBuffer.TryGetValue(serverState.Tick, out var predicted))
            return;

        float error = Vector3.Distance(predicted.Position, serverState.Position);

        // ignore micro errors
        if (error < HardCorrectionThreshold)
            return;

        // HARD SNAP
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
