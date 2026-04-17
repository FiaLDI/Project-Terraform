using System.Collections.Generic;
using Features.Player.UnityIntegration;
using Features.Stats.Adapter;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(DeterministicMovement))]
public class PlayerNetworkController : NetworkBehaviour
{
    private DeterministicMovement movement;
    private MovementInputHandler inputHandler;
    private TimeManager timeManager;
    private MovementStatsAdapter movementStats;

    // CLIENT
    private readonly Dictionary<uint, MoveCommand> inputBuffer = new();
    private readonly Dictionary<uint, PlayerState> predictedStateBuffer = new();

    // SERVER
    private readonly Dictionary<uint, MoveCommand> serverInputBuffer = new();
    private MoveCommand lastServerCmd;
    private MoveCommand lastReceivedServerCmd;

    private uint lastReconciledTick;

    private const int BufferSize = 1024;
    private const uint RemoteClientInputLeadTicks = 6;
    private const float IgnoreReconcileError = 0.5f;
    private const float IdleSnapDistance = 0.9f;
    private const float MovingSnapDistance = 1.5f;
    private const float HardSnapDistance = 12f;
    private const float MovingVerticalSnapDistance = 0.35f;
    private const float HardSnapVerticalDistance = 1.25f;
    private const float VerticalVelocitySnapDelta = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugMovementNet;
    [SerializeField] private bool debugTickFlow;
    [SerializeField] private bool debugOnlyWhenSprinting = true;
    [SerializeField] private float debugLogInterval = 0.5f;
    [SerializeField] private int debugTickEvery = 10;

    private int currentWeaponPose;
    private float nextOwnerDebugTime;
    private float nextServerDebugTime;
    private float nextReconcileDebugTime;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
        movementStats = GetComponent<MovementStatsAdapter>();
    }

    public void InjectInput(MovementInputHandler handler)
    {
        inputHandler = handler;
    }

    public override void OnStartNetwork()
    {
        timeManager = InstanceFinder.TimeManager;
        timeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        if (timeManager != null)
            timeManager.OnTick -= OnTick;

        inputBuffer.Clear();
        predictedStateBuffer.Clear();
        serverInputBuffer.Clear();
        lastServerCmd = default;
        lastReceivedServerCmd = default;
    }

    public void SetWeaponPose(int pose)
    {
        currentWeaponPose = pose;
    }

    private void OnTick()
    {
        if (!IsSpawned)
            return;

        uint tick = timeManager.Tick;
        bool hasOwnerCommand = false;
        MoveCommand ownerCommand = default;

        if (IsOwner)
        {
            if (inputHandler == null)
                return;

            var input = inputHandler.ConsumeState();
            uint commandTick = GetCommandTick(tick);
            var cmd = CreateCommand(commandTick, input);
            hasOwnerCommand = true;
            ownerCommand = cmd;

            inputBuffer[commandTick] = cmd;

            movement.Simulate(cmd);
            predictedStateBuffer[commandTick] = CaptureState(commandTick, cmd);
            MaybeLogOwnerTick(commandTick, cmd);
            MaybeLogOwnerTickFlow(tick, commandTick, cmd);

            if (!IsServer)
                SendInputServerRpc(cmd);

            CleanupOldInputs(tick);
        }

        if (IsServer)
        {
            string commandSource = hasOwnerCommand ? "owner-live" : string.Empty;
            MoveCommand cmd = hasOwnerCommand ? ownerCommand : ResolveServerCommand(tick, out commandSource);
            lastServerCmd = cmd;

            if (!hasOwnerCommand)
                movement.Simulate(cmd);

            var state = hasOwnerCommand && predictedStateBuffer.TryGetValue(cmd.Tick, out var predictedState)
                ? predictedState
                : CaptureState(cmd.Tick, cmd);
            MaybeLogServerTick(cmd.Tick, cmd, state, hasOwnerCommand);
            MaybeLogServerTickFlow(tick, cmd, state, commandSource);
            SendStateObserversRpc(state);

            CleanupServerInputs(tick);
        }
    }

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        serverInputBuffer[cmd.Tick] = cmd;
        lastReceivedServerCmd = cmd;
        MaybeLogServerReceiveTick(cmd);
    }

    [ObserversRpc]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (IsOwner && !IsServer)
        {
            Reconcile(state);
            return;
        }

        GetComponentInChildren<RemoteInterpolation>()?.ReceiveState(state);
    }

    private void Reconcile(PlayerState serverState)
    {
        if (serverState.Tick <= lastReconciledTick)
            return;

        lastReconciledTick = serverState.Tick;

        if (!inputBuffer.ContainsKey(serverState.Tick) ||
            !predictedStateBuffer.TryGetValue(serverState.Tick, out var predictedState))
            return;

        Vector3 fullCorrection = serverState.Position - predictedState.Position;
        Vector2 planarCorrection = new Vector2(fullCorrection.x, fullCorrection.z);
        float planarError = planarCorrection.magnitude;
        float verticalError = Mathf.Abs(fullCorrection.y);
        float verticalVelocityError = Mathf.Abs(serverState.VerticalVelocity - predictedState.VerticalVelocity);
        bool groundedMismatch = serverState.Grounded != predictedState.Grounded;
        MaybeLogReconcile(serverState, predictedState, planarError, verticalError);
        MaybeLogReconcileTickFlow(serverState, predictedState, planarError, verticalError);

        if (planarError < IgnoreReconcileError && verticalError < 0.2f)
            return;

        bool hugeVerticalMismatch = verticalError >= HardSnapVerticalDistance;
        bool hugePlanarMismatch = planarError >= HardSnapDistance;
        bool activelyMoving = IsActivelyMoving(serverState.Tick, predictedState, serverState);
        bool physicsMismatch =
            groundedMismatch ||
            verticalError >= MovingVerticalSnapDistance ||
            verticalVelocityError >= VerticalVelocitySnapDelta;

        // While the owner is actively moving, allow small drift but still
        // correct quickly once physics state or groundedness diverge.
        if (activelyMoving &&
            !hugeVerticalMismatch &&
            !hugePlanarMismatch &&
            planarError < MovingSnapDistance &&
            !physicsMismatch)
            return;

        if (!activelyMoving && !hugeVerticalMismatch && planarError < IdleSnapDistance)
            return;

        movement.ApplyState(serverState);
        predictedStateBuffer[serverState.Tick] = serverState;
        ReplayFromTick(serverState.Tick + 1);
    }

    private bool IsActivelyMoving(uint tick, PlayerState predictedState, PlayerState serverState)
    {
        if (inputBuffer.TryGetValue(tick, out var cmd) && cmd.Move.sqrMagnitude > 0.01f)
            return true;

        Vector2 predictedPlanarVelocity = new(predictedState.Velocity.x, predictedState.Velocity.z);
        if (predictedPlanarVelocity.sqrMagnitude > 0.25f)
            return true;

        Vector2 serverPlanarVelocity = new(serverState.Velocity.x, serverState.Velocity.z);
        return serverPlanarVelocity.sqrMagnitude > 0.25f;
    }

    private void ReplayFromTick(uint startTick)
    {
        uint latestPredictedTick = 0;
        bool hasBufferedInput = false;

        foreach (var bufferedTick in inputBuffer.Keys)
        {
            if (!hasBufferedInput || bufferedTick > latestPredictedTick)
            {
                latestPredictedTick = bufferedTick;
                hasBufferedInput = true;
            }
        }

        if (!hasBufferedInput || startTick > latestPredictedTick)
            return;

        for (uint t = startTick; t <= latestPredictedTick; t++)
        {
            if (!inputBuffer.TryGetValue(t, out var cmd))
                continue;

            movement.Simulate(cmd);
            predictedStateBuffer[t] = CaptureState(t, cmd);
        }
    }

    private void CleanupOldInputs(uint currentTick)
    {
        uint minTick = currentTick > BufferSize ? currentTick - BufferSize : 0;

        var keys = new List<uint>(inputBuffer.Keys);
        foreach (var key in keys)
        {
            if (key < minTick)
                inputBuffer.Remove(key);
        }

        keys = new List<uint>(predictedStateBuffer.Keys);
        foreach (var key in keys)
        {
            if (key < minTick)
                predictedStateBuffer.Remove(key);
        }
    }

    private void CleanupServerInputs(uint currentTick)
    {
        uint minTick = currentTick > BufferSize ? currentTick - BufferSize : 0;

        var keys = new List<uint>(serverInputBuffer.Keys);
        foreach (var key in keys)
        {
            if (key < minTick)
                serverInputBuffer.Remove(key);
        }
    }

    private MoveCommand ResolveServerCommand(uint tick, out string source)
    {
        if (serverInputBuffer.TryGetValue(tick, out var exact))
        {
            source = "exact";
            return exact;
        }

        uint bestTick = 0;
        bool foundBuffered = false;

        foreach (var bufferedTick in serverInputBuffer.Keys)
        {
            if (bufferedTick > tick)
                continue;

            if (!foundBuffered || bufferedTick > bestTick)
            {
                bestTick = bufferedTick;
                foundBuffered = true;
            }
        }

        if (foundBuffered && serverInputBuffer.TryGetValue(bestTick, out var buffered))
        {
            source = "past-buffer";
            return buffered;
        }

        uint nearestFutureTick = uint.MaxValue;
        bool foundFuture = false;

        foreach (var bufferedTick in serverInputBuffer.Keys)
        {
            if (bufferedTick < tick)
                continue;

            if (!foundFuture || bufferedTick < nearestFutureTick)
            {
                nearestFutureTick = bufferedTick;
                foundFuture = true;
            }
        }

        if (foundFuture && serverInputBuffer.TryGetValue(nearestFutureTick, out var future))
        {
            source = "future-buffer";
            return future;
        }

        if (lastReceivedServerCmd.Tick != 0)
        {
            source = "last-received";
            return lastReceivedServerCmd;
        }

        if (lastServerCmd.Tick != 0)
        {
            source = "last-server";
            return lastServerCmd;
        }

        source = "empty";
        return new MoveCommand { Tick = tick };
    }

    private void ShiftPredictedStates(uint startTick, Vector3 correction)
    {
        if (correction.sqrMagnitude <= 0f)
            return;

        var keys = new List<uint>(predictedStateBuffer.Keys);
        foreach (var key in keys)
        {
            if (key < startTick)
                continue;

            var predicted = predictedStateBuffer[key];
            predicted.Position += correction;
            predictedStateBuffer[key] = predicted;
        }
    }

    private MoveCommand CreateCommand(uint tick, PlayerInputState input)
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

    private uint GetCommandTick(uint localTick)
    {
        if (IsServer)
            return localTick;

        return localTick + RemoteClientInputLeadTicks;
    }

    private PlayerState CaptureState(uint tick, MoveCommand cmd)
    {
        return new PlayerState
        {
            Tick = tick,
            Position = transform.position,
            Velocity = movement.Velocity,
            Yaw = cmd.Yaw,
            Pitch = cmd.Pitch,
            VerticalVelocity = movement.VerticalVelocityInternal,
            Grounded = movement.Grounded,
            Crouch = movement.IsCrouching,
            WeaponPose = currentWeaponPose
        };
    }


    private void MaybeLogOwnerTick(uint tick, MoveCommand cmd)
    {
        if (!debugMovementNet || !ShouldLog(Time.unscaledTime, ref nextOwnerDebugTime))
            return;

        if (debugOnlyWhenSprinting && !cmd.Sprint)
            return;

        Debug.Log(
            $"[MoveNet][OWNER] {name} tick={tick} sprint={cmd.Sprint} move={cmd.Move} " +
            $"walk={movementStats?.WalkSpeed:0.##} sprintSpeed={movementStats?.SprintSpeed:0.##} " +
            $"currentMax={movement.CurrentMaxSpeed:0.##} velXZ={new Vector2(movement.Velocity.x, movement.Velocity.z).magnitude:0.##}",
            this);
    }

    private void MaybeLogServerTick(uint tick, MoveCommand cmd, PlayerState state, bool usedOwnerCommand)
    {
        if (!debugMovementNet || !ShouldLog(Time.unscaledTime, ref nextServerDebugTime))
            return;

        if (debugOnlyWhenSprinting && !cmd.Sprint)
            return;

        string source = usedOwnerCommand ? "owner" : "buffer";
        Debug.Log(
            $"[MoveNet][SERVER] {name} tick={tick} source={source} sprint={cmd.Sprint} move={cmd.Move} " +
            $"walk={movementStats?.WalkSpeed:0.##} sprintSpeed={movementStats?.SprintSpeed:0.##} " +
            $"stateVelXZ={new Vector2(state.Velocity.x, state.Velocity.z).magnitude:0.##} pos={state.Position}",
            this);
    }

    private void MaybeLogReconcile(PlayerState serverState, PlayerState predictedState, float planarError, float verticalError)
    {
        if (!debugMovementNet || !ShouldLog(Time.unscaledTime, ref nextReconcileDebugTime))
            return;

        bool sprinting = inputBuffer.TryGetValue(serverState.Tick, out var cmd) && cmd.Sprint;
        if (debugOnlyWhenSprinting && !sprinting)
            return;

        Debug.Log(
            $"[MoveNet][RECONCILE] {name} tick={serverState.Tick} sprint={sprinting} " +
            $"planarError={planarError:0.###} verticalError={verticalError:0.###} " +
            $"predGrounded={predictedState.Grounded} srvGrounded={serverState.Grounded} " +
            $"predVY={predictedState.VerticalVelocity:0.###} srvVY={serverState.VerticalVelocity:0.###} " +
            $"pred={predictedState.Position} srv={serverState.Position} " +
            $"walk={movementStats?.WalkSpeed:0.##} sprintSpeed={movementStats?.SprintSpeed:0.##}",
            this);
    }

    private bool ShouldLog(float now, ref float nextLogTime)
    {
        if (now < nextLogTime)
            return false;

        float interval = Mathf.Max(0.1f, debugLogInterval);
        nextLogTime = now + interval;
        return true;
    }

    private void MaybeLogOwnerTickFlow(uint localTick, uint commandTick, MoveCommand cmd)
    {
        if (!ShouldLogTickFlow(commandTick, cmd))
            return;

        Debug.Log(
            $"[MoveNet][TICK][OWNER] {name} localTick={localTick} commandTick={commandTick} " +
            $"lead={GetSignedTickDelta(commandTick, localTick)} move={cmd.Move} sprint={cmd.Sprint}",
            this);
    }

    private void MaybeLogServerReceiveTick(MoveCommand cmd)
    {
        if (!ShouldLogTickFlow(cmd.Tick, cmd))
            return;

        uint serverTick = timeManager != null ? timeManager.Tick : 0u;
        Debug.Log(
            $"[MoveNet][TICK][SERVER-RX] {name} serverTick={serverTick} cmdTick={cmd.Tick} " +
            $"delta={GetSignedTickDelta(cmd.Tick, serverTick)} move={cmd.Move} sprint={cmd.Sprint}",
            this);
    }

    private void MaybeLogServerTickFlow(uint serverTick, MoveCommand cmd, PlayerState state, string commandSource)
    {
        if (!ShouldLogTickFlow(cmd.Tick, cmd))
            return;

        Debug.Log(
            $"[MoveNet][TICK][SERVER] {name} serverTick={serverTick} cmdTick={cmd.Tick} stateTick={state.Tick} " +
            $"source={commandSource} cmdVsSrv={GetSignedTickDelta(cmd.Tick, serverTick)} " +
            $"velXZ={new Vector2(state.Velocity.x, state.Velocity.z).magnitude:0.##} pos={state.Position}",
            this);
    }

    private void MaybeLogReconcileTickFlow(PlayerState serverState, PlayerState predictedState, float planarError, float verticalError)
    {
        if (!debugTickFlow)
            return;

        bool hasCmd = inputBuffer.TryGetValue(serverState.Tick, out var cmd);
        if (!ShouldLogTickFlow(serverState.Tick, hasCmd ? cmd : default))
            return;

        uint localTick = timeManager != null ? timeManager.Tick : 0u;
        Debug.Log(
            $"[MoveNet][TICK][RECONCILE] {name} localTick={localTick} stateTick={serverState.Tick} " +
            $"stateVsLocal={GetSignedTickDelta(serverState.Tick, localTick)} " +
            $"hasInput={hasCmd} predPos={predictedState.Position} srvPos={serverState.Position} " +
            $"planarError={planarError:0.###} verticalError={verticalError:0.###}",
            this);
    }

    private bool ShouldLogTickFlow(uint tick, MoveCommand cmd)
    {
        if (!debugTickFlow)
            return false;

        if (debugOnlyWhenSprinting && !cmd.Sprint)
            return false;

        uint sampleEvery = (uint)Mathf.Max(1, debugTickEvery);
        return tick % sampleEvery == 0;
    }

    private static int GetSignedTickDelta(uint targetTick, uint referenceTick)
    {
        return unchecked((int)(targetTick - referenceTick));
    }
}
