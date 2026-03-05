using FishNet.Object;
using UnityEngine;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Buffs.Domain;

public sealed class PlayerUsageNetAdapter : NetworkBehaviour
{
    [SerializeField] private float aimSendRate = 20f;

    private Vector3 serverAimOrigin;
    private Vector3 serverAimForward = Vector3.forward;
    private bool hasServerAim;

    private float nextAimSendTime;

    // ======================================================
    // RUNTIME
    // ======================================================

    private EquipmentRuntime equipmentRuntime;

    private ItemInstance rightHandInstance;
    private ItemRuntimeContext activeRuntime;

    private IBuffSource source;

    // ======================================================
    // INIT
    // ======================================================

    private void Awake()
    {
        source = GetComponent<IBuffSource>();
        equipmentRuntime = new EquipmentRuntime(source);
    }

    // ======================================================
    // HANDS
    // ======================================================

    public void OnHandsUpdated(GameObject left, GameObject right, bool twoHanded)
    {
        rightHandInstance = null;

        if (right != null)
        {
            var holder = right.GetComponent<ItemRuntimeHolder>();

            if (holder != null)
                rightHandInstance = holder.Instance;
        }
    }

    public bool TryGetServerAim(out Ray ray)
    {
        ray = new Ray(serverAimOrigin, serverAimForward);
        return hasServerAim;
    }

    // ======================================================
    // ACTION START
    // ======================================================

    public void ActionStart(ItemActionType action)
    {
        if (!IsOwner)
            return;

        if (IsServerInitialized)
        {
            ExecuteAction(action);
            return;
        }

        ActionStart_Server(action);
    }

    [ServerRpc]
    private void ActionStart_Server(ItemActionType action)
    {
        ExecuteAction(action);
    }

    // ======================================================
    // ACTION STOP
    // ======================================================

    public void ActionStop(ItemActionType action)
    {
        if (!IsOwner)
            return;

        if (IsServerInitialized)
        {
            StopAction(action);
            return;
        }

        ActionStop_Server(action);
    }

    [ServerRpc]
    private void ActionStop_Server(ItemActionType action)
    {
        StopAction(action);
    }

    // ======================================================
    // EXECUTION
    // ======================================================

    private void ExecuteAction(ItemActionType action)
    {
        if (rightHandInstance == null || rightHandInstance.IsEmpty)
            return;

        if (!TryGetServerAim(out var ray))
            return;

        activeRuntime = equipmentRuntime.GetRuntime(
            rightHandInstance,
            action
        );

        if (activeRuntime == null)
            return;

        activeRuntime.StartUse(ray.origin, ray.direction);
    }

    private void StopAction(ItemActionType action)
    {
        if (activeRuntime == null)
            return;

        activeRuntime.StopUse();
        activeRuntime = null;
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        // SERVER — continuous aim update
        if (IsServerInitialized && activeRuntime != null)
        {
            if (TryGetServerAim(out var ray))
                activeRuntime.UpdateAim(ray.origin, ray.direction);
        }

        // CLIENT — send aim
        if (!IsOwner)
            return;

        SendAimToServerThrottled();
    }

    // ======================================================
    // AIM SYNC
    // ======================================================

    private void SendAimToServerThrottled()
    {
        if (Time.time < nextAimSendTime)
            return;

        nextAimSendTime = Time.time + (1f / Mathf.Max(1f, aimSendRate));

        var cam = Camera.main;

        if (cam == null)
            return;

        UpdateAim_Server(
            cam.transform.position,
            cam.transform.forward
        );
    }

    [ServerRpc(RequireOwnership = true)]
    private void UpdateAim_Server(Vector3 origin, Vector3 forward)
    {
        serverAimOrigin = origin;

        serverAimForward =
            forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : Vector3.forward;

        hasServerAim = true;
    }

    // ======================================================
    // SYNC HANDS
    // ======================================================

    [ServerRpc]
    public void SyncHands_Server()
    {
        // var equip = GetComponent<EquipmentManager>();

        // rightHand = equip.GetRightHandUsable();
        // leftHand = equip.GetLeftHandUsable();
    }
}
