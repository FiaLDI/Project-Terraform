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

    private int lastProcessedTick = -1;
    private const int InputDelay = 2;
    private MoveCommand lastServerCmd;
    private bool hasServerCmd;
    private MoveCommand currentCmd;
    private int currentWeaponPose;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
    }

    public void InjectInput(MovementInputHandler handler)
    {
        inputHandler = handler;
    }

    public void SetWeaponPose(int pose)
    {
        if (IsOwner && !IsServer)
            SendWeaponPoseServerRpc(pose);
        currentWeaponPose = pose;
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

        if (IsOwner)
        {
            if (inputHandler == null)
                return;

            var input = inputHandler.ConsumeState();
            var cmd = CreateCommand(tick, input);

            currentCmd = cmd;

            if (!IsServer)
            {
                movement.Simulate(cmd);
                SendInputServerRpc(cmd);
            }
        }
        
        if (IsServer)
        {
            var cmd = currentCmd;

            cmd.Tick = tick;

            movement.Simulate(cmd);

            var state = CaptureState(tick, cmd);

            SendStateObserversRpc(state);
        }
    }

    [ServerRpc]
    private void SendInputServerRpc(MoveCommand cmd)
    {
        currentCmd = cmd;
    }

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
            Crouch = movement.IsCrouching,
            WeaponPose = currentWeaponPose
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

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(PlayerState state)
    {
        if (base.IsOwner && !base.IsServer)
            return;

        GetComponentInChildren<RemoteInterpolation>()
            ?.ReceiveState(state);
    }

    [ServerRpc]
    private void SendWeaponPoseServerRpc(int pose)
    {
        currentWeaponPose = pose;
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
