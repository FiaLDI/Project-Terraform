using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Application;
using Features.Inventory.UnityIntegration;
using Features.Stats.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Глобальный реестр игроков.
    /// Не отвечает за готовность статов.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayerRegistry : MonoBehaviour
    {
        public static PlayerRegistry Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PlayerRegistry>();
                return _instance;
            }
            private set => _instance = value;
        }
        private static PlayerRegistry _instance;

        // ================= PLAYERS =================

        private readonly List<GameObject> _players = new();
        public IReadOnlyList<GameObject> Players => _players;

        // ================= LOCAL PLAYER =================

        public GameObject LocalPlayer { get; private set; }

        public InventoryManager LocalInventory { get; private set; }
        public AbilityCaster LocalAbilities { get; private set; }
        public PlayerStats LocalPlayerStats { get; private set; }

        public static event Action<PlayerRegistry> OnLocalPlayerReady;

        public bool HasLocalPlayer => LocalPlayer != null;

        // ================= LIFECYCLE =================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("[PlayerRegistry] Initialized as singleton", this);
        }

        // ================= REGISTRATION =================

        public void RegisterPlayer(GameObject player)
        {
            if (player == null || _players.Contains(player))
                return;

            _players.Add(player);
            Debug.Log($"[PlayerRegistry] Registered player: {player.name}", this);
        }

        public void UnregisterPlayer(GameObject player)
        {
            if (player == null)
                return;

            _players.Remove(player);

            if (LocalPlayer == player)
                ClearLocalPlayer();

            Debug.Log($"[PlayerRegistry] Unregistered player: {player.name}", this);
        }

        // ================= LOCAL PLAYER =================

        /// <summary>
        /// Вызывается для локального игрока.
        /// Гарантирует только наличие ссылок, не данных.
        /// </summary>
        public void SetLocalPlayer(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("[PlayerRegistry] SetLocalPlayer called with NULL", this);
                return;
            }

            if (LocalPlayer == player)
                return;

            LocalPlayer = player;
            Debug.Log($"[PlayerRegistry] Local player set: {player.name}", this);

            // -------- cache components --------
            LocalInventory = player.GetComponent<InventoryManager>();
            if (LocalInventory == null)
                Debug.LogWarning("[PlayerRegistry] InventoryManager not found on local player", player);

            LocalAbilities = player.GetComponent<AbilityCaster>();
            if (LocalAbilities == null)
                Debug.LogWarning("[PlayerRegistry] AbilityCaster not found on local player", player);

            LocalPlayerStats = player.GetComponent<PlayerStats>();
            if (LocalPlayerStats == null)
                Debug.LogWarning("[PlayerRegistry] PlayerStats not found on local player", player);

            // -------- notify --------
            Debug.Log("[PlayerRegistry] Invoking OnLocalPlayerReady", this);
            OnLocalPlayerReady?.Invoke(this);
        }

        private void ClearLocalPlayer()
        {
            Debug.Log("[PlayerRegistry] Local player cleared", this);

            LocalPlayer = null;
            LocalInventory = null;
            LocalAbilities = null;
            LocalPlayerStats = null;
        }

        // ================= UTIL =================

        /// <summary>
        /// Возвращает PlayerStats локального игрока, если есть.
        /// НЕ проверяет готовность данных.
        /// </summary>
        public PlayerStats GetLocalStats()
        {
            if (LocalPlayerStats == null)
            {
                Debug.LogWarning("[PlayerRegistry] LocalPlayerStats is null", this);
                return null;
            }

            return LocalPlayerStats;
        }

        public static void SubscribeLocalPlayerReady(Action<PlayerRegistry> cb)
        {
            OnLocalPlayerReady += cb;

            // если локальный игрок уже есть — вызываем сразу
            if (Instance != null && Instance.LocalPlayer != null)
                cb(Instance);
        }

        public static void UnsubscribeLocalPlayerReady(Action<PlayerRegistry> cb)
        {
            OnLocalPlayerReady -= cb;
        }
    }
}
