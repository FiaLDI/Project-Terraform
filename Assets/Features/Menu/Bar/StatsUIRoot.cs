using Features.Player.UI;
using Features.UI;
using UnityEngine;

namespace Features.Stats.UI
{
    /// <summary>
    /// Управляет привязкой игрока к UI компонентам статов (бары энергии, здоровья)
    /// </summary>
    public class StatsUIRoot : MonoBehaviour
    {
        private PlayerBoundUIView[] statsUIComponents;
        private GameObject boundPlayer;

        private void Awake()
        {
            // Получаем все PlayerBoundUIView компоненты в этом Root
            statsUIComponents = GetComponentsInChildren<PlayerBoundUIView>(true);
            Debug.Log($"[StatsUIRoot] Found {statsUIComponents.Length} UI components", this);
        }

        private void Start()
        {
            // Подписываемся на событие привязки игрока из PlayerUIRoot
            var playerUIRoot = PlayerUIRoot.I;
            if (playerUIRoot != null)
            {
                Debug.Log("[StatsUIRoot] Subscribing to PlayerUIRoot.OnPlayerBound", this);
                playerUIRoot.OnPlayerBound += OnPlayerBound;

                // Если игрок уже привязан - привязываем сразу
                if (playerUIRoot.BoundPlayer != null)
                {
                    Debug.Log("[StatsUIRoot] PlayerUIRoot already has BoundPlayer, binding now", this);
                    OnPlayerBound(playerUIRoot.BoundPlayer);
                }
            }
            else
            {
                Debug.LogError("[StatsUIRoot] PlayerUIRoot.I is null!", this);
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от события
            if (PlayerUIRoot.I != null)
            {
                PlayerUIRoot.I.OnPlayerBound -= OnPlayerBound;
            }
        }

        /// <summary>
        /// Вызывается когда игрок привязан к PlayerUIRoot
        /// </summary>
        private void OnPlayerBound(GameObject player)
        {
            Debug.Log($"[StatsUIRoot] OnPlayerBound: {player.name}", this);

            if (player == null)
            {
                boundPlayer = null;
                UnbindAll();
                return;
            }

            boundPlayer = player;

            // Привязываем всех компонентов к игроку
            foreach (var uiComponent in statsUIComponents)
            {
                if (uiComponent != null && uiComponent.gameObject.activeInHierarchy)
                {
                    Debug.Log($"[StatsUIRoot] Binding {uiComponent.GetType().Name}", this);
                    
                    // Вызываем защищённый метод OnPlayerBound через рефлексию
                    var methodInfo = uiComponent.GetType().GetMethod(
                        "OnPlayerBound",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                    );

                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(uiComponent, new object[] { player });
                    }
                    else
                    {
                        Debug.LogWarning($"[StatsUIRoot] OnPlayerBound method not found on {uiComponent.GetType().Name}", uiComponent);
                    }
                }
            }
        }

        /// <summary>
        /// Отвязывает всех компонентов от игрока
        /// </summary>
        private void UnbindAll()
        {
            foreach (var uiComponent in statsUIComponents)
            {
                if (uiComponent != null)
                {
                    Debug.Log($"[StatsUIRoot] Unbinding {uiComponent.GetType().Name}", this);

                    var methodInfo = uiComponent.GetType().GetMethod(
                        "OnPlayerUnbound",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                    );

                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(uiComponent, new object[] { boundPlayer });
                    }
                }
            }
        }
    }
}