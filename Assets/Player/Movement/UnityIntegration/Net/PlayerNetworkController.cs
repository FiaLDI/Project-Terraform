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

    // 🔥 INPUT DELAY
    private const int InputDelayTicks = 2;

    // 🔥 SMOOTH CORRECTION
    private Vector3 pendingCorrection;

    private bool isTeleporting;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
    }

    public void InjectInput(MovementInputHandler handler)
    {
        inputHandler = handler;
    }

    private void Update()
    {
        if (!IsOwner || inputHandler == null)
            return;

        float yaw = inputHandler.CurrentState.Yaw;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
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
        pendingCorrection = Vector3.zero;
    }

    private void OnTick()
    {
        if (!IsSpawned)
            return;

        int currentTick = NetworkTickSystem.I.CurrentTick;

        // ================= CLIENT =================
        if (IsOwner)
        {
            if (inputHandler == null)
                return;

            var input = inputHandler.CurrentState;

            int delayedTick = currentTick + InputDelayTicks;

            MoveCommand cmd = new MoveCommand
            {
                Tick   = delayedTick,
                Move   = input.Move,
                Yaw    = input.Yaw,
                Pitch  = input.Pitch,
                Jump   = input.Jump,
                Crouch = input.Crouch,
                Sprint = input.Sprint
            };

            inputBuffer[delayedTick] = cmd;

            // prediction
            if (!IsServer)
            {
                movement.Simulate(cmd);

                stateBuffer[delayedTick] = new PlayerState
                {
                    Tick     = delayedTick,
                    Position = transform.position,
                    Velocity = movement.Velocity,
                    Yaw      = transform.eulerAngles.y
                };
            }

            SendInputServerRpc(cmd);
            inputHandler.ClearOneShotFlags();
        }

        // ================= SERVER =================
        if (IsServer)
        {
            int simulationTick = currentTick;

            if (!inputBuffer.TryGetValue(simulationTick, out var cmd))
            {
                int latestTick = -1;

                foreach (var kvp in inputBuffer)
                {
                    if (kvp.Key > latestTick)
                        latestTick = kvp.Key;
                }

                cmd = latestTick != -1 ? inputBuffer[latestTick] : default;
            }

            movement.Simulate(cmd);

            PlayerState state = new PlayerState
            {
                Tick     = simulationTick,
                Position = transform.position,
                Velocity = movement.Velocity,
                Yaw      = transform.eulerAngles.y,
                Pitch    = cmd.Pitch,
                Grounded = movement.Grounded,
                Crouch   = movement.IsCrouching,
                Jump     = movement.JumpedThisTick
            };

            SendStateObserversRpc(state);
            SendStateTargetRpc(Owner, state);

            int cleanupTick = simulationTick - 100;
            inputBuffer.Remove(cleanupTick);
        }

        // ================= SMOOTH CORRECTION =================
        if (!IsServer && IsOwner)
        {
            if (pendingCorrection.sqrMagnitude > 0.0001f)
            {
                Vector3 step = pendingCorrection * 0.15f;

                movement.AddExternalVelocity(step);

                pendingCorrection -= step;

                if (pendingCorrection.magnitude < 0.01f)
                    pendingCorrection = Vector3.zero;
            }
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
        if (IsOwner)
            return;

        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    [TargetRpc]
    private void SendStateTargetRpc(NetworkConnection conn, PlayerState serverState)
    {
        // 🔥 хост не корректим
        if (IsServer)
            return;

        // 🔥 teleport режим
        if (isTeleporting)
        {
            movement.Teleport(
                serverState.Position,
                serverState.Yaw,
                serverState.Velocity.y
            );

            inputBuffer.Clear();
            stateBuffer.Clear();
            pendingCorrection = Vector3.zero;

            isTeleporting = false;
            return;
        }

        float error = Vector3.Distance(transform.position, serverState.Position);

        // 🔥 HARD SNAP
        if (error > HardSnapThreshold)
        {
            movement.Teleport(
                serverState.Position,
                serverState.Yaw,
                serverState.Velocity.y
            );

            inputBuffer.Clear();
            stateBuffer.Clear();
            pendingCorrection = Vector3.zero;
            return;
        }

        // 🔥 МЯГКАЯ КОРРЕКЦИЯ
        if (error > IgnoreErrorThreshold)
        {
            Vector3 correction = serverState.Position - transform.position;

            // защита от скачков
            if (correction.magnitude > 3f)
            {
                movement.Teleport(
                    serverState.Position,
                    serverState.Yaw,
                    serverState.Velocity.y
                );

                pendingCorrection = Vector3.zero;
                return;
            }

            pendingCorrection += correction;
        }
    }

    // ================= TELEPORT =================

    [ServerRpc]
    public void RequestReturnToSpawnServerRpc()
    {
        if (!PlayerSpawnRegistry.I.TryGetSpawnPoint(out var pos, out var rot))
            return;

        TeleportTo(pos, rot);
    }

    [Server]
    private void TeleportTo(Vector3 position, Quaternion rotation)
    {
        inputBuffer.Clear();
        stateBuffer.Clear();

        movement.Teleport(position, rotation.eulerAngles.y, 0f);

        int tick = NetworkTickSystem.I.CurrentTick;

        PlayerState state = new PlayerState
        {
            Tick     = tick,
            Position = transform.position,
            Velocity = movement.Velocity,
            Yaw      = transform.eulerAngles.y,
            Pitch    = 0f,
            Grounded = movement.Grounded,
            Crouch   = movement.IsCrouching,
            Jump     = false
        };

        isTeleporting = true;

        SendStateObserversRpc(state);
        SendStateTargetRpc(Owner, state);
    }

    // ================= QUEST RPC =================

    [ServerRpc]
    public void RequestReturnToHubServerRpc()
    {
        SceneTransitionService.LoadHubScene();
    }

    // ================= WORLD / QUEST RPC =================
    [ServerRpc]
    public void RequestWorldServerRpc(
        int seed,
        List<string> questIds,
        List<string> chainIds)
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
