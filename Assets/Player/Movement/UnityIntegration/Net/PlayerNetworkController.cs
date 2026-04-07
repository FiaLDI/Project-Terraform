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

    private const int InputDelayTicks = 1;
    private Vector3 visualPosition;

    private Vector3 accumulatedError;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 currentPosition;
    private Quaternion currentRotation;

    public Vector3 GetPreviousPosition() => previousPosition;
    public Vector3 GetCurrentPosition() => currentPosition;

    public Quaternion GetPreviousRotation() => previousRotation;
    public Quaternion GetCurrentRotation() => currentRotation;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
        visualPosition = transform.position;

        previousPosition = transform.position;
        currentPosition  = transform.position;

        previousRotation = transform.rotation;
        currentRotation  = transform.rotation;
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
    
        previousPosition = currentPosition;
        previousRotation = currentRotation;

        currentPosition = transform.position;
        currentRotation = transform.rotation;

        // ================= CLIENT =================
        if (IsOwner)
        {
            if (inputHandler == null)
                return;

            var input = inputHandler.CurrentState;

            int tick = currentTick + InputDelayTicks;

            MoveCommand cmd = new MoveCommand
            {
                Tick   = tick,
                Move   = input.Move,
                Yaw    = input.Yaw,
                Pitch  = input.Pitch,
                Jump   = input.Jump,
                Crouch = input.Crouch,
                Sprint = input.Sprint
            };

            inputBuffer[tick] = cmd;

            // 🔥 CLIENT PREDICTION
            if (!IsServer)
            {
                movement.Simulate(cmd);

                stateBuffer[tick] = new PlayerState
                {
                    Tick     = tick,
                    Position = transform.position,
                    Velocity = movement.Velocity,

                    Yaw      = movement.CurrentYawInternal,
                    Pitch    = input.Pitch,

                    VerticalVelocity = movement.VerticalVelocityInternal,
                    InternalYaw      = movement.CurrentYawInternal,

                    Grounded = movement.Grounded,
                    Crouch   = movement.IsCrouching
                };
            }

            SendInputServerRpc(cmd);
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

                Yaw      = movement.CurrentYawInternal,
                Pitch    = cmd.Pitch,

                VerticalVelocity = movement.VerticalVelocityInternal,
                InternalYaw      = movement.CurrentYawInternal,

                Grounded = movement.Grounded,
                Crouch   = movement.IsCrouching
            };

            SendStateObserversRpc(state);
            SendStateTargetRpc(Owner, state);

            inputBuffer.Remove(simulationTick - 100);
        }

        const int BUFFER_LIMIT = 256;

        if (inputBuffer.Count > BUFFER_LIMIT)
        {
            int oldTick = currentTick - BUFFER_LIMIT;
            inputBuffer.Remove(oldTick);
        }

        if (stateBuffer.Count > BUFFER_LIMIT)
        {
            int oldTick = currentTick - BUFFER_LIMIT;
            stateBuffer.Remove(oldTick);
        }
    }

    // ================= INPUT =================

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        inputBuffer[cmd.Tick] = cmd;
    }

    // ================= REMOTE =================

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (IsOwner)
            return;

        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    // ================= RECONCILIATION =================

    [TargetRpc]
    private void SendStateTargetRpc(NetworkConnection conn, PlayerState serverState)
    {
        if (IsServer)
            return;

        // если нет состояния — просто применяем
        if (!stateBuffer.TryGetValue(serverState.Tick, out var predicted))
        {
            movement.ApplyState(serverState);
            return;
        }

        const float MinCorrectionError = 0.2f;

        float error = Vector3.Distance(predicted.Position, serverState.Position);

        if (error < MinCorrectionError)
            return;

        const float HardSnapThreshold = 3f;

        if (error > HardSnapThreshold)
        {
            movement.ApplyState(serverState);
        }
        else
        {
            Vector3 correction = serverState.Position - transform.position;

            accumulatedError += correction;

            Vector3 step = accumulatedError * 0.1f;

            // clamp чтобы не было рывков
            step = Vector3.ClampMagnitude(step, 0.5f);

            movement.ApplyCorrection(step);

            accumulatedError -= step;
            return;
        }

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

                    Yaw      = movement.CurrentYawInternal,
                    Pitch    = cmd.Pitch,

                    VerticalVelocity = movement.VerticalVelocityInternal,
                    InternalYaw      = movement.CurrentYawInternal,

                    Grounded = movement.Grounded,
                    Crouch   = movement.IsCrouching
                };
            }
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

        PlayerState state = new PlayerState
        {
            Tick     = NetworkTickSystem.I.CurrentTick,
            Position = transform.position,
            Velocity = movement.Velocity,

            Yaw      = movement.CurrentYawInternal,
            Pitch    = 0f,

            VerticalVelocity = movement.VerticalVelocityInternal,
            InternalYaw      = movement.CurrentYawInternal,

            Grounded = movement.Grounded,
            Crouch   = movement.IsCrouching
        };

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
