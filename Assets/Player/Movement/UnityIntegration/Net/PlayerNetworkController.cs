using System.Collections.Generic;
using Features.Player.UnityIntegration;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing.Timing;
using UnityEngine;
using FishNet;

[RequireComponent(typeof(DeterministicMovement))]
public class PlayerNetworkController : NetworkBehaviour
{
    private DeterministicMovement movement;
    private MovementInputHandler inputHandler;

    private TimeManager timeManager;

    // CLIENT
    private readonly Dictionary<uint, MoveCommand> inputBuffer = new();

    // SERVER
    private readonly Dictionary<uint, MoveCommand> serverInputBuffer = new();
    private MoveCommand lastServerCmd;

    private uint lastReconciledTick;

    private const int BufferSize = 1024;
    private const float IgnoreReconcileError = 0.2f;
    private const float SoftReconcileError = 0.35f;
    private const float SoftCorrectionLimit = 0.06f;
    private int currentWeaponPose = 0;

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
        timeManager = InstanceFinder.TimeManager;
        timeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        if (timeManager != null)
            timeManager.OnTick -= OnTick;

        inputBuffer.Clear();
        serverInputBuffer.Clear();
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

        // ================= CLIENT =================
        if (IsOwner)
        {
            if (inputHandler == null)
                return;

            var input = inputHandler.ConsumeState();

            var cmd = CreateCommand(tick, input);

            inputBuffer[tick] = cmd;

            // prediction
            movement.Simulate(cmd);

            // отправка на сервер
            if (!IsServer)
                SendInputServerRpc(cmd);

            CleanupOldInputs(tick);
        }

        // ================= SERVER =================
        if (IsServer)
        {
            MoveCommand cmd;

            if (!serverInputBuffer.TryGetValue(tick, out cmd))
            {
                // fallback (если пакет потерян)
                if (lastServerCmd.Tick != 0)
                {
                    cmd = lastServerCmd;
                    cmd.Tick = tick;
                }
                else
                {
                    cmd = new MoveCommand { Tick = tick };
                }
            }

            lastServerCmd = cmd;

            // 🔥 сервер ВСЕГДА симулирует
            movement.Simulate(cmd);

            var state = CaptureState(tick, cmd);

            SendStateObserversRpc(state);

            CleanupServerInputs(tick);
        }
    }

    // ================= RPC =================

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        serverInputBuffer[cmd.Tick] = cmd;
    }

    [ObserversRpc]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (IsOwner && !IsServer)
        {
            Reconcile(state);
            return;
        }

        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    // ================= RECONCILIATION =================

    private void Reconcile(PlayerState serverState)
    {
        if (serverState.Tick <= lastReconciledTick)
            return;

        lastReconciledTick = serverState.Tick;

        if (!inputBuffer.ContainsKey(serverState.Tick))
            return;

        float error = Vector3.Distance(transform.position, serverState.Position);

        if (error < IgnoreReconcileError)
            return;

        if (error < SoftReconcileError)
        {
            Vector3 correction = serverState.Position - transform.position;
            correction = Vector3.ClampMagnitude(correction, SoftCorrectionLimit);

            movement.ApplyCorrection(correction);
            ReplayFromTick(serverState.Tick + 1);
            return;
        }

        // жёсткий rollback
        movement.ApplyState(serverState);
        ReplayFromTick(serverState.Tick + 1);
    }

    private void ReplayFromTick(uint startTick)
    {
        uint currentTick = timeManager.Tick;

        for (uint t = startTick; t <= currentTick; t++)
        {
            if (inputBuffer.TryGetValue(t, out var cmd))
            {
                movement.Simulate(cmd);
            }
        }
    }

    // ================= CLEANUP =================

    private void CleanupOldInputs(uint currentTick)
    {
        uint minTick = currentTick > BufferSize ? currentTick - BufferSize : 0;

        var keys = new List<uint>(inputBuffer.Keys);
        foreach (var key in keys)
        {
            if (key < minTick)
                inputBuffer.Remove(key);
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

    // ================= DATA =================

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
}
