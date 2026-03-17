using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;
using Features.Player.UnityIntegration;
using System.Collections;

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

    private void Update()
    {
        if (!IsOwner)
            return;

        if (inputHandler == null)
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
    }

    private void OnTick()
    {
        if (!IsSpawned)
            return;

        int currentTick = NetworkTickSystem.I.CurrentTick;

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
                Pitch  = input.Pitch,
                Jump   = input.Jump,
                Crouch = input.Crouch,
                Sprint = input.Sprint
            };

            inputBuffer[currentTick] = cmd;

            if (!IsServer)
            {
                movement.Simulate(cmd);

                stateBuffer[currentTick] = new PlayerState
                {
                    Tick     = currentTick,
                    Position = transform.position,
                    Velocity = movement.Velocity,
                    Yaw      = transform.eulerAngles.y
                };
            }

            SendInputServerRpc(cmd);
            inputHandler.ClearOneShotFlags();
        }

        // ================= SERVER SIMULATION =================
        if (IsServer)
        {
            int simulationTick = currentTick;
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
                Yaw      = transform.eulerAngles.y,
                Pitch    = cmd.Pitch,
                Grounded = movement.Grounded,
                Crouch   = movement.IsCrouching,
                Jump     = movement.JumpedThisTick
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

        if (error < IgnoreErrorThreshold)
            return;

        movement.Teleport(
            serverState.Position,
            serverState.Yaw,
            serverState.Velocity.y
        );

        // 🔥 Удаляем старые состояния
        stateBuffer.Remove(serverState.Tick);

        // 🔥 Пересимулируем input после этого тика
        int currentTick = NetworkTickSystem.I.CurrentTick;

        for (int tick = serverState.Tick + 1; tick <= currentTick; tick++)
        {
            if (inputBuffer.TryGetValue(tick, out var cmd))
            {
                movement.Simulate(cmd);

                stateBuffer[tick] = new PlayerState
                {
                    Tick     = tick,
                    Position = transform.position,
                    Velocity = movement.Velocity,
                    Yaw      = transform.eulerAngles.y
                };
            }
        }
    }

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
    public void RequestReturnToHubServerRpc()
    {
        SceneTransitionService.LoadHubScene();
    }

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

        SendStateObserversRpc(state);
        SendStateTargetRpc(Owner, state);
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
