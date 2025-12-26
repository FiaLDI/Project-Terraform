using System;
using Features.Inventory.UI;
using Features.Player.UI;
using UnityEngine;

namespace Features.Bootstrap
{
    /// <summary>
    /// Контролирует порядок инициализации систем на сцене.
    /// Гарантирует, что UI подписывается на события ПОСЛЕ того, как
    /// системы-источники (PlayerUIRoot) инициализированы.
    /// 
    /// Вешать этот скрипт на объект, который ПЕРВЫЙ инициализируется на сцене.
    /// </summary>
    public sealed class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private InventoryUIView inventoryUI;
        [SerializeField] private PlayerUIRoot playerUIRoot;

        private void Awake()
        {
            Debug.Log("[SceneBootstrap] Starting initialization");
            
            // Этап 1: Убедиться, что системы-источники уже инициализированы
            if (playerUIRoot == null)
            {
                playerUIRoot = FindObjectOfType<PlayerUIRoot>();
            }
            
            if (playerUIRoot == null)
            {
                Debug.LogError("[SceneBootstrap] PlayerUIRoot not found!");
                return;
            }
            
            Debug.Log("[SceneBootstrap] PlayerUIRoot found");

            // Этап 2: Отключить UI, который зависит от систем
            if (inventoryUI != null)
            {
                Debug.Log("[SceneBootstrap] Disabling InventoryUI before initialization");
                inventoryUI.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            Debug.Log("[SceneBootstrap] Start phase - enabling dependent systems");
            
            // Этап 3: Включить UI, чтобы его OnEnable сработал ПОСЛЕ инициализации источников
            if (inventoryUI != null && playerUIRoot != null)
            {
                Debug.Log("[SceneBootstrap] Enabling InventoryUI");
                inventoryUI.gameObject.SetActive(true);
            }
        }

        // Альтернативный вариант: явная инициализация (если система не автоматическая)
        // Раскомментируй, если нужна более тонкая контроль
        /*
        private void Start()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[SceneBootstrap] Initializing systems in order");
            
            // 1. Убедиться, что PlayerUIRoot готов
            if (playerUIRoot == null)
            {
                playerUIRoot = FindObjectOfType<PlayerUIRoot>();
            }
            
            if (playerUIRoot == null)
            {
                Debug.LogError("[SceneBootstrap] PlayerUIRoot not found!");
                return;
            }
            
            Debug.Log("[SceneBootstrap] 1. PlayerUIRoot initialized");

            // 2. Инициализировать UI, которые зависят от систем
            if (inventoryUI != null)
            {
                // Проверим - может, InventoryUI уже инициализировался?
                // Если нет - явно вызовем его инициализацию
                Debug.Log("[SceneBootstrap] 2. InventoryUI initialized");
            }
        }
        */
    }
}
