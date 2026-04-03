using UnityEngine;
using FishNet.Object;
using System.Collections;
using Features.Stats.Domain;
using FishNet.Connection;

public class StatsDebugStreamer : NetworkBehaviour
{
    private Coroutine streamRoutine;

    // ================= PUBLIC =================

    public void StartStreaming()
    {
        if (!IsOwner)
            return;

        StartStreamServer();
    }

    public void StopStreaming()
    {
        if (!IsOwner)
            return;

        StopStreamServer();
    }

    // ================= SERVER =================

    [ServerRpc]
    private void StopStreamServer(NetworkConnection conn = null)
    {
        if (streamRoutine == null)
            return;

        StopCoroutine(streamRoutine);
        streamRoutine = null;

        StopClient(conn);
    }

    [ServerRpc]
    private void StartStreamServer(NetworkConnection conn = null)
    {
        if (streamRoutine != null)
            return;

        streamRoutine = StartCoroutine(StreamRoutine(conn));
    }

    private IEnumerator StreamRoutine(NetworkConnection conn)
    {
        while (true)
        {
            var data = BuildData();
            SendToClient(conn, data);

            yield return new WaitForSeconds(0.5f);
        }
    }

    private StatsDebugData BuildData()
    {
        var buffTarget = GetComponent<StatsBuffTarget>();
        if (buffTarget == null)
            return default;

        var stats = buffTarget.GetServerStats();
        if (stats == null)
            return default;

        return new StatsDebugData
        {
            damage = stats.Combat?.FinalDamage ?? 0f,
            fireRate = stats.Combat?.FireRate ?? 0f,
            spread = stats.Combat?.Spread ?? 0f,
            aimSpread = stats.Combat?.AimSpread ?? 0f,
            range = stats.Combat?.Range ?? 0f,
            recoil = stats.Combat?.Recoil ?? 0f,
            magazine = stats.Combat?.MagazineSize ?? 0,

            critChance = stats.Combat?.CritChance ?? 0f,
            critMultiplier = stats.Combat?.CritMultiplier ?? 0f,
            penetration = stats.Combat?.Penetration ?? 0f,

            hp = stats.Health?.CurrentHp ?? 0f,
            maxHp = stats.Health?.MaxHp ?? 0f,
            shield = stats.Health?.CurrentShield ?? 0f,
            maxShield = stats.Health?.MaxShield ?? 0f,

            energy = stats.Energy?.CurrentEnergy ?? 0f,
            maxEnergy = stats.Energy?.MaxEnergy ?? 0f,
            regen = stats.Energy?.Regen ?? 0f,
            costMult = stats.Energy?.CostMultiplier ?? 1f,

            walk = stats.Movement?.WalkSpeed ?? 0f,
            sprint = stats.Movement?.SprintSpeed ?? 0f,
            crouch = stats.Movement?.CrouchSpeed ?? 0f,
            rotation = stats.Movement?.RotationSpeed ?? 0f,
            gravity = stats.Movement?.Gravity ?? 0f,
            jump = stats.Movement?.JumpHeight ?? 0f,

            generic = stats.Protect?.GenericResistance ?? 0f,
            explosion = stats.Protect?.ExplosionResistance ?? 0f,
            energyRes = stats.Protect?.EnergyResistance ?? 0f,
            mining = stats.Protect?.MiningResistance ?? 0f,
            melee = stats.Protect?.MeleeResistance ?? 0f,
            fire = stats.Protect?.FireResistance ?? 0f,
            electric = stats.Protect?.ElectricResistance ?? 0f,
            poison = stats.Protect?.PoisonResistance ?? 0f,
            frost = stats.Protect?.FrostResistance ?? 0f,
            acid = stats.Protect?.AcidResistance ?? 0f,
        };
    }

    [TargetRpc]
    private void SendToClient(NetworkConnection conn, StatsDebugData data)
    {
        var ui = FindAnyObjectByType<StatsAdvancedDebugUI>();
        ui?.ApplyServerData(data);
    }

    [TargetRpc]
    private void StopClient(NetworkConnection conn)
    {
        var ui = FindAnyObjectByType<StatsAdvancedDebugUI>();
        ui?.DisableServerMode();
    }
}
