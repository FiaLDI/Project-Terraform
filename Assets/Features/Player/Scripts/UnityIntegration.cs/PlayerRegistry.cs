using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Application;
using Features.Inventory.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Глобальный реестр игроков.
    /// ❗ НЕ хранит статы.
    /// ❗ НЕ решает, кто локальный — это делает Net-слой.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayerRegistry : MonoBehaviour
    {
        public static PlayerRegistry Instance { get; private set; }

        // Все игроки в сцене
        private readonly List<GameObject> _players = new();
        public IReadOnlyList<GameObject> Players => _players;

        // Локальный игрок (ТОЛЬКО 1)
        public GameObject LocalPlayer { get; private set; }

        // Кэш локальных подсистем (НЕ глобальные!)
        public InventoryManager LocalInventory { get; private set; }
        public AbilityCaster LocalAbilities { get; private set; }

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

            Instance = this;
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

            Debug.Log($"[PlayerRegistry] Registered player: {player.name}");
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

            Debug.Log($"[PlayerRegistry] Unregistered player: {player.name}");
        }

        // ======================================================
        // LOCAL PLAYER
        // ======================================================

        /// <summary>
        /// Вызывается ТОЛЬКО из Net-слоя (NetworkPlayer / NetAdapter)
        /// </summary>
        public void SetLocalPlayer(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("[PlayerRegistry] SetLocalPlayer called with NULL");
                return;
            }

            if (LocalPlayer == player)
                return;

            LocalPlayer = player;

            // Кэшируем ТОЛЬКО удобные ссылки
            LocalInventory = player.GetComponent<InventoryManager>();
            LocalAbilities = player.GetComponent<AbilityCaster>();

            _localPlayerInitialized = true;

            Debug.Log($"[PlayerRegistry] Local player set: {player.name}");

            OnLocalPlayerReady?.Invoke(this);
        }

        private void ClearLocalPlayer()
        {
            Debug.Log("[PlayerRegistry] Local player cleared");

            LocalPlayer = null;
            LocalInventory = null;
            LocalAbilities = null;
            _localPlayerInitialized = false;
        }
    }
}
