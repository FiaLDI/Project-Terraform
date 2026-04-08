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
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private Quaternion previousRotation;
    private Quaternion currentRotation;

public Vector3 GetPreviousPosition() => previousPosition;
public Vector3 GetCurrentPosition() => currentPosition;
public Quaternion GetPreviousRotation() => previousRotation;
public Quaternion GetCurrentRotation() => currentRotation;

    private int lastProcessedTick = -1;

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
        NetworkTickSystem.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        NetworkTickSystem.OnTick -= OnTick;
        inputBuffer.Clear();
        stateBuffer.Clear();
    }

    private void OnTick()
    {
        if (!IsSpawned)
            return;

        int tick = NetworkTickSystem.I.CurrentTick;

        previousPosition = currentPosition;
        previousRotation = currentRotation;

        currentPosition = transform.position;
        currentRotation = transform.rotation;

        // ======================================================
        // HOST (самый стабильный путь)
        // ======================================================
        if (IsOwner && IsServer)
        {
            var input = inputHandler.CurrentState;

            MoveCommand cmd = CreateCommand(tick, input);

            movement.Simulate(cmd);
            return;
        }

        // ======================================================
        // CLIENT (prediction)
        // ======================================================
        if (IsOwner)
        {
            var input = inputHandler.CurrentState;

            MoveCommand cmd = CreateCommand(tick, input);

            inputBuffer[tick] = cmd;

            // prediction
            movement.Simulate(cmd);

            stateBuffer[tick] = CaptureState(tick, cmd);

            SendInputServerRpc(cmd);
        }

        // ======================================================
        // SERVER
        // ======================================================
        if (IsServer)
        {
            if (!inputBuffer.TryGetValue(tick, out var cmd))
            {
                // ❗ ВАЖНО: используем последний валидный, а не return
                if (!inputBuffer.TryGetValue(lastProcessedTick, out cmd))
                {
                    cmd = default;
                }
            }

            movement.Simulate(cmd);
            lastProcessedTick = tick;

            var state = CaptureState(tick, cmd);

            SendStateObserversRpc(state);
            SendStateTargetRpc(Owner, state);
        }
    }

    // ======================================================
    // INPUT
    // ======================================================

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        inputBuffer[cmd.Tick] = cmd;
    }

    // ======================================================
    // STATE
    // ======================================================

    private PlayerState CaptureState(int tick, MoveCommand cmd)
    {
        return new PlayerState
        {
            Tick = tick,
            Position = transform.position,
            Velocity = movement.Velocity,

            Yaw = movement.CurrentYawInternal,
            Pitch = cmd.Pitch,

            VerticalVelocity = movement.VerticalVelocityInternal,
            InternalYaw = movement.CurrentYawInternal,

            Grounded = movement.Grounded,
            Crouch = movement.IsCrouching
        };
    }

    private MoveCommand CreateCommand(int tick, PlayerInputState input)
    {
        return new MoveCommand
        {
            Tick = tick,
            Move = input.Move,
            Yaw = input.Yaw,
            Pitch = input.Pitch,
            Jump = input.Jump,
            Crouch = input.Crouch,
            Sprint = input.Sprint
        };
    }

    // ======================================================
    // REMOTE
    // ======================================================

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (IsOwner)
            return;

        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    // ======================================================
    // RECONCILIATION
    // ======================================================

    [TargetRpc]
    private void SendStateTargetRpc(NetworkConnection conn, PlayerState serverState)
    {
        if (IsServer)
            return;

        if (!stateBuffer.TryGetValue(serverState.Tick, out var predicted))
        {
            movement.ApplyState(serverState);
            return;
        }

        float error = Vector3.Distance(predicted.Position, serverState.Position);

        // 🔥 анти-дрожание
        if (error < 0.5f)
            return;

        // snap
        movement.ApplyState(serverState);

        int currentTick = NetworkTickSystem.I.CurrentTick;

        // replay
        for (int t = serverState.Tick + 1; t <= currentTick; t++)
        {
            if (inputBuffer.TryGetValue(t, out var cmd))
            {
                movement.Simulate(cmd);
                stateBuffer[t] = CaptureState(t, cmd);
            }
        }
    }

    [Server]
    private void TeleportTo(Vector3 position, Quaternion rotation)
    {
        movement.ApplyState(new PlayerState
        {
            Tick = NetworkTickSystem.I.CurrentTick,
            Position = position,
            Velocity = Vector3.zero,
            Yaw = rotation.eulerAngles.y,
            Pitch = 0f,
            VerticalVelocity = 0f,
            InternalYaw = rotation.eulerAngles.y,
            Grounded = true,
            Crouch = false
        });
    }

    // ================= QUEST / WORLD RPC =================

    [ServerRpc]
    public void RequestReturnToSpawnServerRpc()
    {
        if (!PlayerSpawnRegistry.I.TryGetSpawnPoint(out var pos, out var rot))
            return;

        TeleportTo(pos, rot);
    }

    [ServerRpc]
    public void RequestReturnToHubServerRpc()
    {
        SceneTransitionService.LoadHubScene();
    }

    [ServerRpc]
    public void RequestWorldServerRpc(int seed, List<string> questIds, List<string> chainIds)
    {
        ServerWorldSession.PendingSeed = seed;
        ServerWorldSession.PendingQuestIds = questIds;
        ServerWorldSession.PendingChainIds = chainIds;

        SceneTransitionService.LoadWorldScene();
    }

    [ServerRpc]
    public void GiveQuestsServerRpc(List<string> questIds)
    {
        GetComponent<PlayerQuestComponent>()?.GiveQuests(questIds);
    }

    [ServerRpc]
    public void GiveChainsServerRpc(List<string> chainIds)
    {
        GetComponent<PlayerQuestComponent>()?.GiveChains(chainIds);
    }

    [ServerRpc]
    public void ClearQuestsServerRpc()
    {
        GetComponent<PlayerQuestComponent>()?.ClearAll();
    }

    [ServerRpc]
    public void DebugCompleteQuestServerRpc(string questId)
    {
        GetComponent<PlayerQuestComponent>()?.DebugCompleteQuest(questId);
    }

    [ServerRpc]
    public void DebugFailQuestServerRpc(string questId)
    {
        GetComponent<PlayerQuestComponent>()?.DebugFailQuest(questId);
    }

    [ServerRpc]
    public void DebugAdvanceQuestServerRpc(string questId)
    {
        GetComponent<PlayerQuestComponent>()?.DebugAdvance(questId);
    }
}
