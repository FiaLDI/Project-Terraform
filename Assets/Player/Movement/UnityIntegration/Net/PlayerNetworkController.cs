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
    private const int InputDelayTicks = 2;

    // 🔥 ВАЖНО: визуальная коррекция вместо физической
    private Vector3 visualOffset;

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

        // ================= VISUAL SMOOTHING =================
        if (!IsServer)
        {
            if (visualOffset.sqrMagnitude > 0.0001f)
            {
                float smoothSpeed = 8f;

                Vector3 step = Vector3.Lerp(
                    Vector3.zero,
                    visualOffset,
                    1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)
                );

                // 🔥 фикс дрожания
                if (step.magnitude > visualOffset.magnitude)
                {
                    step = visualOffset;
                }

                transform.position += step;
                visualOffset -= step;

                if (visualOffset.magnitude < 0.01f)
                    visualOffset = Vector3.zero;
            }
        }
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
        visualOffset = Vector3.zero;
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

            inputBuffer.Remove(simulationTick - 100);
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
        if (IsServer)
            return;

        // TELEPORT MODE
        if (isTeleporting)
        {
            Vector3 pos = serverState.Position;
            pos.y = transform.position.y;

            movement.Teleport(pos, serverState.Yaw, serverState.Velocity.y);

            inputBuffer.Clear();
            stateBuffer.Clear();
            visualOffset = Vector3.zero;

            isTeleporting = false;
            return;
        }

        Vector3 flatError = serverState.Position - transform.position;
        flatError.y = 0f;

        float error = flatError.magnitude;

        // HARD SNAP
        if (error > HardSnapThreshold)
        {
            Vector3 pos = serverState.Position;
            pos.y = transform.position.y;

            movement.Teleport(pos, serverState.Yaw, serverState.Velocity.y);

            inputBuffer.Clear();
            stateBuffer.Clear();
            visualOffset = Vector3.zero;
            return;
        }

        if (error < IgnoreErrorThreshold)
            return;

        Vector3 correction = serverState.Position - transform.position;
        correction.y = 0f;

        visualOffset += correction;

        visualOffset = Vector3.ClampMagnitude(visualOffset, 1.5f);
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

    // ================= QUEST / WORLD RPC =================

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
