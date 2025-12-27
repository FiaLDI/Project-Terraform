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
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayerRegistry : MonoBehaviour
    {
        public static PlayerRegistry Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PlayerRegistry>();
                }
                return _instance;
            }
            private set => _instance = value;
        }
        private static PlayerRegistry _instance;


        // Все игроки в сцене
        private readonly List<GameObject> _players = new();
        public IReadOnlyList<GameObject> Players => _players;

        // Локальный игрок (ТОЛЬКО 1)
        public GameObject LocalPlayer { get; private set; }

        // Кэш локальных подсистем
        public InventoryManager LocalInventory { get; private set; }
        public AbilityCaster LocalAbilities { get; private set; }
        public PlayerStats LocalPlayerStats { get; private set; }

        private bool _localPlayerInitialized;

        public static event Action<PlayerRegistry> OnLocalPlayerReady;

        public bool HasLocalPlayer => LocalPlayer != null;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Instance = this;
            Debug.Log("[PlayerRegistry] Initialized as singleton", this);
        }

        // ======================================================
        // PLAYER REGISTRATION
        // ======================================================

        /// <summary>
        /// Регистрирует игрока (любой: локальный или удалённый)
        /// </summary>
        public void RegisterPlayer(GameObject player)
        {
            if (player == null)
                return;

            if (_players.Contains(player))
                return;

            _players.Add(player);
            Debug.Log($"[PlayerRegistry] Registered player: {player.name}", this);
        }

        /// <summary>
        /// Удаляет игрока из реестра
        /// </summary>
        public void UnregisterPlayer(GameObject player)
        {
            if (player == null)
                return;

            _players.Remove(player);

            if (LocalPlayer == player)
            {
                ClearLocalPlayer();
            }

            Debug.Log($"[PlayerRegistry] Unregistered player: {player.name}", this);
        }

        public static void SubscribeLocalPlayerReady(Action<PlayerRegistry> cb)
        {
            OnLocalPlayerReady += cb;

            if (Instance != null && Instance.LocalPlayer != null)
                cb(Instance);
        }

        public static void UnsubscribeLocalPlayerReady(Action<PlayerRegistry> cb)
        {
            OnLocalPlayerReady -= cb;
        }

        // ======================================================
        // LOCAL PLAYER
        // ======================================================

        /// <summary>
        /// Вызывается из NetworkPlayer.OnStartClient() для локального игрока.
        /// ВАЖНО: К этому моменту PlayerStats уже инициализирован!
        /// </summary>
        public void SetLocalPlayer(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("[PlayerRegistry] SetLocalPlayer called with NULL", this);
                return;
            }

            if (LocalPlayer == player)
            {
                Debug.LogWarning("[PlayerRegistry] Local player already set to this player!", this);
                return;
            }

            LocalPlayer = player;
            Debug.Log($"[PlayerRegistry] Local player set: {player.name}", this);

            // Кэшируем ссылки на основные компоненты
            LocalInventory = player.GetComponent<InventoryManager>();
            if (LocalInventory == null)
                Debug.LogWarning("[PlayerRegistry] InventoryManager not found on local player", player);

            LocalAbilities = player.GetComponent<AbilityCaster>();
            if (LocalAbilities == null)
                Debug.LogWarning("[PlayerRegistry] AbilityCaster not found on local player", player);

            LocalPlayerStats = player.GetComponent<PlayerStats>();
            if (LocalPlayerStats == null)
            {
                Debug.LogError("[PlayerRegistry] PlayerStats not found on local player!", player);
            }
            else if (!LocalPlayerStats.IsReady)
            {
                Debug.LogError("[PlayerRegistry] PlayerStats is NOT ready!", player);
            }
            else
            {
                Debug.Log("[PlayerRegistry] PlayerStats initialized successfully ✅", this);
            }

            _localPlayerInitialized = true;

            // Уведомляем подписчиков что локальный игрок готов
            Debug.Log("[PlayerRegistry] Invoking OnLocalPlayerReady event", this);
            OnLocalPlayerReady?.Invoke(this);
        }

        private void ClearLocalPlayer()
        {
            Debug.Log("[PlayerRegistry] Local player cleared", this);

            LocalPlayer = null;
            LocalInventory = null;
            LocalAbilities = null;
            LocalPlayerStats = null;
            _localPlayerInitialized = false;
        }

        // ======================================================
        // UTILITIES
        // ======================================================

        /// <summary>
        /// Получить статы локального игрока (безопасно).
        /// </summary>
        public PlayerStats GetLocalStats()
        {
            if (LocalPlayerStats == null || !LocalPlayerStats.IsReady)
            {
                Debug.LogError("[PlayerRegistry] Local player stats not ready!", this);
                return null;
            }
            return LocalPlayerStats;
        }
    }
}
