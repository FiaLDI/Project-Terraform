using FishNet.Object;
using UnityEngine;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Buffs.Domain;
using Features.Player.UnityIntegration;

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

    private Transform worldMuzzle;
    private Transform viewMuzzle;
    private PlayerCameraController cam;

    private bool isFPS;

    private void Awake()
    {
        source = GetComponent<IBuffSource>();
        equipmentRuntime = new EquipmentRuntime(source);
        cam = GetComponent<PlayerCameraController>();
    }
    public void SetMuzzles(Transform world, Transform view)
    {
        worldMuzzle = world;
        viewMuzzle = view;
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

        Vector3 hitPoint;

        if (Physics.Raycast(ray, out var hit, 1000f))
            hitPoint = hit.point;
        else
            hitPoint = ray.origin + ray.direction * 1000f;

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

        bool isFPS = cam != null && cam.IsFPS();

        Vector3 fireOrigin;

        if (IsOwner && isFPS && viewMuzzle != null)
        {
            fireOrigin = viewMuzzle.position;
        }
        else
        {
            fireOrigin = worldMuzzle != null
                ? worldMuzzle.position
                : ray.origin;
        }

        activeRuntime.UpdateAim(fireOrigin, hitPoint, true);

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
        if (activeRuntime != null)
        {
            // ================= SERVER (другие игроки / не владелец) =================
            if (IsServerInitialized && !IsOwner && TryGetServerAim(out var serverRay))
            {
                Vector3 hitPoint;

                if (Physics.Raycast(serverRay, out var hit, 1000f))
                    hitPoint = hit.point;
                else
                    hitPoint = serverRay.origin + serverRay.direction * 1000f;

                Vector3 fireOrigin = worldMuzzle != null
                    ? worldMuzzle.position
                    : serverRay.origin;

                activeRuntime.UpdateAim(fireOrigin, hitPoint, true);
            }

            // ================= LOCAL PLAYER =================
            else if (IsOwner)
            {
                var camMain = Camera.main;
                if (camMain == null)
                    return;

                Ray camRay = camMain.ViewportPointToRay(new Vector3(0.5f, 0.5f));

                Vector3 hitPoint;

                if (Physics.Raycast(camRay, out var hit, 1000f))
                    hitPoint = hit.point;
                else
                    hitPoint = camRay.origin + camRay.direction * 1000f;

                // 🔥 ОПРЕДЕЛЯЕМ FPS / TPS
                bool isFPS = false;

                var camController = GetComponent<PlayerCameraController>();
                if (camController != null)
                    isFPS = camController.IsFPS();

                Vector3 fireOrigin;

                if (isFPS && viewMuzzle != null)
                {
                    // FPS → из рук
                    fireOrigin = viewMuzzle.position;
                }
                else
                {
                    // TPS → из world оружия
                    fireOrigin = worldMuzzle != null
                        ? worldMuzzle.position
                        : camRay.origin;
                }

                activeRuntime.UpdateAim(fireOrigin, hitPoint, true);
            }
        }

        // ================= SEND AIM =================
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
