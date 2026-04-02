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

    private EquipmentRuntime equipmentRuntime;

    private ItemInstance rightHandInstance;
    private ItemRuntimeContext activeRuntime;
    private ItemRuntimeHolder rightHandHolder;

    [SerializeField] private GameObject viewModelRoot;

    private IBuffSource source;

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
        rightHandHolder = null;

        if (right != null)
        {
            var holder = right.GetComponent<ItemRuntimeHolder>();

            if (holder != null)
            {
                rightHandInstance = holder.Instance;
                rightHandHolder = holder;
            }
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

    private void ExecuteAction(ItemActionType action)
    {
        if (rightHandInstance == null || rightHandInstance.IsEmpty)
            return;

        if (rightHandHolder == null)
            return;

        if (!TryGetServerAim(out var ray))
            return;

        // 🎯 hitPoint
        Vector3 hitPoint;

        if (Physics.Raycast(ray, out var hit, 1000f))
            hitPoint = hit.point;
        else
            hitPoint = ray.origin + ray.direction * 1000f;

        // 🔥 CLIENT PREDICTION
        if (IsOwner)
            PlayViewModelFx();

        activeRuntime = equipmentRuntime.GetRuntime(
            rightHandInstance,
            action,
            rightHandHolder
        );

        if (activeRuntime == null)
            return;

        activeRuntime.OnFire = (pos, dir) =>
        {
            if (IsServer)
                RpcPlayWorldFx(pos, dir);
        };

        activeRuntime.StartUse(hitPoint);
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
        if (IsServerInitialized && activeRuntime != null)
        {
            if (TryGetServerAim(out var ray))
            {
                Vector3 hitPoint;

                if (Physics.Raycast(ray, out var hit, 1000f))
                    hitPoint = hit.point;
                else
                    hitPoint = ray.origin + ray.direction * 1000f;

                activeRuntime.UpdateAim(ray.origin, hitPoint, true);
            }
        }

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

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        UpdateAim_Server(ray.origin, ray.direction);
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
    // FX
    // ======================================================

    [ObserversRpc]
    private void RpcPlayWorldFx(Vector3 pos, Vector3 dir)
    {
        if (IsOwner)
            return;

        PlayWorldFx(pos, dir);
    }

    private void PlayViewModelFx()
    {
        if (viewModelRoot == null)
            return;

        Debug.Log("VIEWMODEL FIRE");

        // TODO:
        // muzzle flash
        // recoil
        // animation
    }

    private void PlayWorldFx(Vector3 pos, Vector3 dir)
    {
        Debug.DrawRay(pos, dir * 2f, Color.yellow, 0.5f);

        // TODO:
        // particle
        // sound
        // tracer
    }

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

    public bool HasWeapon()
    {
        return rightHandInstance != null && !rightHandInstance.IsEmpty;
    }
}
