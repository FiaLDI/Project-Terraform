using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Buffs.Domain;
using Features.Player.UnityIntegration;
using Features.Equipment.UnityIntegration;
using Features.Effects.Application;
using Features.Effects.Domain;
using Features.Weapons.Domain;
using Features.Items.Data;
using Features.Camera.UnityIntegration;

public sealed class PlayerUsageNetAdapter : NetworkBehaviour
{
    [SerializeField] private float aimSendRate = 20f;

    private Vector3 serverAimOrigin;
    private Vector3 serverAimForward = Vector3.forward;
    private bool hasServerAim;

    private float nextAimSendTime;

    private EquipmentRuntime equipmentRuntime;
    private ItemInstance activeItemInstance;
    private ItemRuntimeContext activeRuntime;

    private IBuffSource source;

    private Transform worldMuzzle;
    private Transform viewMuzzle;

    private Camera cachedCam;
    private PlayerCameraController camController;

    private readonly Dictionary<GameObject, IProjectileVisual> visualCache = new();

    private void Awake()
    {
        source = GetComponent<IBuffSource>();
        equipmentRuntime = new EquipmentRuntime(source);

        cachedCam = Camera.main;
        camController = GetComponent<PlayerCameraController>();
    }

    // ======================================================
    // SETUP
    // ======================================================

    public void SetMuzzles(Transform world, Transform view)
    {
        worldMuzzle = world;
        viewMuzzle = view;
    }

    public void OnEquippedItemUpdated(GameObject equipped)
    {
        ItemInstance nextInstance = null;

        if (equipped != null)
        {
            var holder = equipped.GetComponent<ItemRuntimeHolder>();
            if (holder != null)
                nextInstance = holder.Instance;
        }

        if (ReferenceEquals(activeItemInstance, nextInstance))
            return;

        if (activeRuntime != null)
        {
            activeRuntime.StopUse();
            activeRuntime = null;
        }

        if (activeItemInstance != null && !activeItemInstance.IsEmpty)
            equipmentRuntime.Remove(activeItemInstance);

        activeItemInstance = nextInstance;
    }

    public bool TryGetServerAim(out Ray ray)
    {
        ray = new Ray(serverAimOrigin, serverAimForward);
        return hasServerAim;
    }

    // ======================================================
    // AIM
    // ======================================================

    private bool TryGetAimData(out Vector3 origin, out Vector3 direction, out Vector3 hitPoint)
    {
        origin = default;
        direction = default;
        hitPoint = default;

        if (!TryResolveAimCamera())
            return false;

        Ray camRay = cachedCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(camRay, out var camHit, 1000f))
            hitPoint = camHit.point;
        else
            hitPoint = camRay.origin + camRay.direction * 1000f;

        bool isFPS = camController != null && camController.IsFPS();

        origin = isFPS && viewMuzzle != null
            ? viewMuzzle.position
            : worldMuzzle != null
                ? worldMuzzle.position
                : camRay.origin;

        direction = (hitPoint - origin).normalized;

        return true;
    }

    // ======================================================
    // ACTION START
    // ======================================================

    public void ActionStart(ItemActionType action)
    {
        if (!IsOwner)
            return;

        if (TryGetAimData(out _, out _, out var hitPoint))
            PlayLocalShot(action, hitPoint);

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

    private void ExecuteAction(ItemActionType action)
    {
        if (activeItemInstance == null || activeItemInstance.IsEmpty)
            return;

        if (!TryGetServerAim(out var ray))
            return;

        Vector3 hitPoint;

        if (Physics.Raycast(ray, out var hit, 1000f))
            hitPoint = hit.point;
        else
            hitPoint = ray.origin + ray.direction * 1000f;

        activeRuntime = equipmentRuntime.GetRuntime(
            activeItemInstance,
            action,
            null
        );

        if (activeRuntime == null)
            return;

        Vector3 finalHitPoint = hitPoint;
        Vector3 fireOrigin = GetFireOrigin(ray.origin);

        activeRuntime.OnFire = (_, _) =>
        {
            if (!IsServer)
                return;

            var config = GetCurrentProjectileConfig(action);
            if (config != null)
                ServerNotifyShot(fireOrigin, finalHitPoint, action);
        };

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
    // LOCAL VISUAL
    // ======================================================

    private void PlayLocalShot(ItemActionType action, Vector3 hitPoint)
    {
        var config = GetCurrentProjectileConfig(action);
        if (config == null)
            return;

        SpawnVisual(hitPoint, config, true);
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        if (activeRuntime != null)
        {
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
            else if (IsOwner)
            {
                if (TryGetAimData(out var origin, out _, out var hitPoint))
                    activeRuntime.UpdateAim(origin, hitPoint, true);
            }
        }

        if (!IsOwner)
            return;

        SendAimToServerThrottled();
    }

    // ======================================================
    // FIRE ORIGIN
    // ======================================================

    private Vector3 GetFireOrigin(Vector3 fallback)
    {
        bool isFPS = camController != null && camController.IsFPS();

        if (IsOwner && isFPS && viewMuzzle != null)
            return viewMuzzle.position;

        return worldMuzzle != null
            ? worldMuzzle.position
            : fallback;
    }

    // ======================================================
    // AIM SYNC
    // ======================================================

    private void SendAimToServerThrottled()
    {
        if (Time.time < nextAimSendTime)
            return;

        nextAimSendTime = Time.time + (1f / Mathf.Max(1f, aimSendRate));

        if (!TryResolveAimCamera())
            return;

        Ray ray = cachedCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        UpdateAim_Server(ray.origin, ray.direction);
    }

    [ServerRpc(RequireOwnership = true)]
    private void UpdateAim_Server(Vector3 origin, Vector3 forward)
    {
        serverAimOrigin = origin;
        serverAimForward = forward.sqrMagnitude > 0.0001f
            ? forward.normalized
            : Vector3.forward;

        hasServerAim = true;
    }

    // ======================================================
    // RPC VISUAL
    // ======================================================

    [ObserversRpc]
    private void RpcPlayWorldFx(Vector3 spawnPos, Vector3 hitPoint, string itemId, ItemActionType actionType)
    {
        if (IsOwner)
            return;

        var item = ItemRegistrySO.Instance.Get(itemId);
        if (item == null)
            return;

        var config = ExtractProjectileConfig(item, actionType);
        if (config == null)
            return;

        SpawnVisual(spawnPos, hitPoint, config);
    }

    [Server]
    public void ServerNotifyShot(Vector3 spawnPos, Vector3 hitPoint, ItemActionType actionType)
    {
        string itemId = activeItemInstance?.itemDefinition?.id;

        if (string.IsNullOrEmpty(itemId))
            return;

        RpcPlayWorldFx(spawnPos, hitPoint, itemId, actionType);
    }

    // ======================================================
    // VISUAL
    // ======================================================

    private void SpawnVisual(Vector3 hitPoint, ProjectileConfig config, bool isOwner)
    {
        if (config == null || config.clientProjectilePrefab == null)
            return;

        Vector3 spawnPos = GetVisualSpawnPosition();
        SpawnVisual(spawnPos, hitPoint, config);
    }

    private void SpawnVisual(Vector3 spawnPos, Vector3 hitPoint, ProjectileConfig config)
    {
        if (config == null || config.clientProjectilePrefab == null)
            return;

        if (ProjectilePool.Instance == null)
        {
            Debug.LogWarning("[PlayerUsageNetAdapter] ProjectilePool missing, cannot spawn visual.", this);
            return;
        }

        Quaternion rot = config.visualType == ProjectileVisualType.Projectile
            ? Quaternion.LookRotation((hitPoint - spawnPos).normalized)
            : Quaternion.identity;

        var go = ProjectilePool.Instance.Get(
            config.clientProjectilePrefab,
            spawnPos,
            rot
        );

        if (!visualCache.TryGetValue(go, out var visual))
        {
            visual = go.GetComponent<IProjectileVisual>();
            visualCache[go] = visual;
        }

        if (visual != null)
            visual.Init(spawnPos, hitPoint, config.lifetime);
        else
            Debug.LogError("NO IProjectileVisual ON PREFAB");
    }

    private Vector3 GetVisualSpawnPosition()
    {
        bool isFPS = camController != null && camController.IsFPS();

        return isFPS && viewMuzzle != null
            ? viewMuzzle.position
            : worldMuzzle != null
                ? worldMuzzle.position
                : transform.position;
    }

    private bool TryResolveAimCamera()
    {
        if (cachedCam != null && cachedCam.isActiveAndEnabled)
            return true;

        var registryCamera = CameraRegistry.Instance?.CurrentCamera;
        if (registryCamera != null && registryCamera.isActiveAndEnabled)
        {
            cachedCam = registryCamera;
            return true;
        }

        cachedCam = Camera.main;
        return cachedCam != null && cachedCam.isActiveAndEnabled;
    }

    // ======================================================
    // CONFIG
    // ======================================================

    private ProjectileConfig GetCurrentProjectileConfig(ItemActionType actionType)
    {
        var action = GetCurrentActionDefinition(actionType);
        if (action?.effects == null)
            return null;

        foreach (var effect in action.effects)
        {
            if (effect.type == EffectType.SpawnProjectile)
                return effect.projectileConfig;
        }

        return null;
    }

    private ItemActionDefinition GetCurrentActionDefinition(ItemActionType actionType)
    {
        var equip = GetComponent<EquipmentManager>();
        var equipped = equip?.GetEquippedObject();

        var holder = equipped != null
            ? equipped.GetComponent<ItemRuntimeHolder>()
            : null;

        return FindActionDefinition(holder?.Instance?.itemDefinition, actionType);
    }

    private static ProjectileConfig ExtractProjectileConfig(Item item, ItemActionType actionType)
    {
        var action = FindActionDefinition(item, actionType);
        if (action?.effects == null)
            return null;

        foreach (var effect in action.effects)
        {
            if (effect.type == EffectType.SpawnProjectile)
                return effect.projectileConfig;
        }

        return null;
    }

    private static ItemActionDefinition FindActionDefinition(Item item, ItemActionType actionType)
    {
        if (item?.actions == null)
            return null;

        foreach (var action in item.actions)
        {
            if (action.actionType == actionType)
                return action;
        }

        return null;
    }

    [Server]
    public void ServerNotifyShot(Vector3 spawnPos, Vector3 hitPoint)
    {
        ServerNotifyShot(spawnPos, hitPoint, ItemActionType.Primary);
    }

    // ======================================================
    // STOP
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

    public bool HasWeapon()
    {
        return activeItemInstance != null && !activeItemInstance.IsEmpty;
    }
}

