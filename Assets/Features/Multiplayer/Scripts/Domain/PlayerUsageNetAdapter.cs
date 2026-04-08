using FishNet.Object;
using UnityEngine;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Buffs.Domain;
using Features.Player.UnityIntegration;
using Features.Equipment.UnityIntegration;
using Features.Effects.Domain;
using Features.Weapons.Domain;
using Features.Items.Data;
using System.Collections.Generic;

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

    public void SetMuzzles(Transform world, Transform view)
    {
        worldMuzzle = world;
        viewMuzzle = view;
    }

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

    private bool TryGetAimData(out Vector3 origin, out Vector3 direction, out Vector3 hitPoint)
    {
        origin = default;
        direction = default;
        hitPoint = default;

        if (cachedCam == null)
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

    private void ExecuteAction(ItemActionType action)
    {
        if (rightHandInstance == null || rightHandInstance.IsEmpty)
            return;

        if (!TryGetServerAim(out var ray))
            return;

        Vector3 hitPoint;

        if (Physics.Raycast(ray, out var hit, 1000f))
            hitPoint = hit.point;
        else
            hitPoint = ray.origin + ray.direction * 1000f;

        if (IsOwner)
            PlayViewModelFx(action);

        activeRuntime = equipmentRuntime.GetRuntime(
            rightHandInstance,
            action,
            null
        );

        if (activeRuntime == null)
            return;

        activeRuntime.OnFire = (_, _) =>
        {
            if (!IsServer)
                return;

            if (!TryGetServerAim(out var ray))
                return;

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out var camHit, 1000f))
                targetPoint = camHit.point;
            else
                targetPoint = ray.origin + ray.direction * 1000f;

            Vector3 finalHit = targetPoint;

            ServerNotifyShot(finalHit);
        };

        Vector3 fireOrigin = GetFireOrigin(ray.origin);

        activeRuntime.UpdateAim(fireOrigin, hitPoint, true);
        activeRuntime.StartUse(hitPoint);
    }

    private void PlayViewModelFx(ItemActionType actionType)
    {
        if (!IsOwner)
            return;

        var config = GetCurrentProjectileConfig();
        if (config == null)
            return;

        if (!TryGetAimData(out var origin, out _, out var hitPoint))
            return;

        SpawnVisual(hitPoint, config, true);
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
                {
                    activeRuntime.UpdateAim(origin, hitPoint, true);
                }
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

        if (cachedCam == null)
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
    // FX
    // ======================================================

    [ObserversRpc]
    private void RpcPlayWorldFx(Vector3 hitPoint, string itemId)
    {
        if (IsOwner)
            return;

        var item = ItemRegistrySO.Instance.Get(itemId);
        if (item == null)
            return;

        var config = ExtractProjectileConfig(item);
        if (config == null)
            return;

        SpawnVisual(hitPoint, config, false);
    }

    [Server]
    public void ServerNotifyShot(Vector3 hitPoint)
    {
        string itemId = rightHandInstance?.itemDefinition?.id;

        if (string.IsNullOrEmpty(itemId))
            return;

        RpcPlayWorldFx(hitPoint, itemId);
    }
    
    private void SpawnVisual(
        Vector3 hitPoint,
        ProjectileConfig config,
        bool isOwner)
    {
        if (config == null)
            return;

        Vector3 spawnPos = isOwner && viewMuzzle != null
            ? viewMuzzle.position
            : worldMuzzle != null
                ? worldMuzzle.position
                : transform.position;

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
        {
            visual.Init(spawnPos, hitPoint, config.lifetime);
        }
        else
        {
            Debug.LogError("NO IProjectileVisual ON PREFAB");
        }
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

    private ProjectileConfig GetCurrentProjectileConfig()
    {
        var equip = GetComponent<EquipmentManager>();
        var right = equip?.GetRightHandObject();

        var holder = right != null
            ? right.GetComponent<ItemRuntimeHolder>()
            : null;

        var item = holder?.Instance?.itemDefinition;

        if (item?.actions == null)
            return null;

        foreach (var action in item.actions)
        {
            foreach (var effect in action.effects)
            {
                if (effect.type == EffectType.SpawnProjectile)
                    return effect.projectileConfig;
            }
        }

        return null;
    }

    private ProjectileConfig ExtractProjectileConfig(Item item)
    {
        if (item?.actions == null)
            return null;

        foreach (var action in item.actions)
        {
            foreach (var effect in action.effects)
            {
                if (effect.type == EffectType.SpawnProjectile)
                    return effect.projectileConfig;
            }
        }

        return null;
    }

    public bool HasWeapon()
    {
        return rightHandInstance != null && !rightHandInstance.IsEmpty;
    }
}
