using System.Collections.Generic;
using UnityEngine;
using Features.Stats.Adapter;
using Features.Abilities.Application;
using Features.Inventory.UnityIntegration;
using System;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Глобальный реестр игрока/турелей.
    /// Это UnityIntegration-слой, пересекающийся с другими фичами через адаптеры.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class PlayerRegistry : MonoBehaviour
    {
        public static PlayerRegistry Instance { get; private set; }

        public readonly List<GameObject> PlayerTurrets = new();
        public readonly List<GameObject> Players = new();
        private bool _localPlayerInitialized;

        public GameObject LocalPlayer { get; private set; }

        public static event Action<PlayerRegistry> OnLocalPlayerReady;

        // Адаптеры статов
        public StatsFacadeAdapter LocalStats { get; private set; }

        public HealthStatsAdapter LocalHealth => LocalStats?.HealthStats;
        public EnergyStatsAdapter LocalEnergy => LocalStats?.EnergyStats;
        public CombatStatsAdapter LocalCombat => LocalStats?.CombatStats;
        public MovementStatsAdapter LocalMovement => LocalStats?.MovementStats;
        public MiningStatsAdapter LocalMining => LocalStats?.MiningStats;

        public AbilityCaster LocalAbilities { get; private set; }

        public readonly Dictionary<GameObject, List<GameObject>> PlayerOwnedTurrets
            = new();

        public InventoryManager LocalInventory { get; private set; }


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // =======================================================================
        // РЕГИСТРАЦИЯ ИГРОКА
        // =======================================================================
        public void Register(GameObject player, StatsFacadeAdapter statsAdapter)
        {
            if (!player.CompareTag("Player"))
            {
                Debug.Log("[PlayerRegistry] Ignore non-player stats");
                return;
            }

            if (!Players.Contains(player))
                Players.Add(player);

            if (LocalPlayer == null)
                LocalPlayer = player;

            LocalInventory = player.GetComponent<InventoryManager>();
            LocalStats = statsAdapter;
            LocalAbilities = player.GetComponent<AbilityCaster>();

            if (!_localPlayerInitialized && LocalPlayer != null)
            {
                _localPlayerInitialized = true;
                OnLocalPlayerReady?.Invoke(this);
            }
        }

        public void RegisterTurret(GameObject ownerPlayer, GameObject turret)
        {
            if (!PlayerOwnedTurrets.ContainsKey(ownerPlayer))
                PlayerOwnedTurrets[ownerPlayer] = new List<GameObject>();

            PlayerOwnedTurrets[ownerPlayer].Add(turret);
            PlayerTurrets.Add(turret);
        }

        public void UnregisterTurret(GameObject ownerPlayer, GameObject turret)
        {
            if (PlayerOwnedTurrets.TryGetValue(ownerPlayer, out var list))
                list.Remove(turret);

            PlayerTurrets.Remove(turret);
        }
    }
}
