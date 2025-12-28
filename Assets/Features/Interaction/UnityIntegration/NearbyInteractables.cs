using System.Collections.Generic;
using Features.Interaction.Domain;
using Features.Items.UnityIntegration;
using UnityEngine;
using FishNet.Object;


/// <summary>
/// Управляет списком предметов рядом с игроком.
/// Находит ближайший интерактивный предмет в пределах видимости.
/// 
/// 🟢 Работает ТОЛЬКО для локального игрока (IsOwner)
/// 🟢 Автоматически очищает мёртвые объекты
/// 🟢 Не спамит в консоль при входе других игроков
/// 🟢 Не использует Input напрямую - просто управляет списком
/// </summary>
public class NearbyInteractables : MonoBehaviour, INearbyInteractables
{
    [Header("Tuning")]
    [SerializeField] private float maxDistance = 3.0f;
    [SerializeField] private float maxAngle = 45f;
    [SerializeField] private int cleanupInterval = 60; // очистка каждые 60 фреймов


    private readonly List<WorldItemNetwork> items = new();
    private int frameCounter = 0;

    // 🟢 Network проверки
    private NetworkObject networkObject;
    private bool isLocalPlayer = false;


    /* ================= LIFECYCLE ================= */


    private void Awake()
    {
        // 🟢 Получаем NetworkObject родителя (Player)
        networkObject = GetComponentInParent<NetworkObject>();
        
        if (networkObject == null)
        {
            Debug.LogError("[NearbyInteractables] NetworkObject not found on parent!", this);
            enabled = false;
            return;
        }

        Debug.Log("[NearbyInteractables] Awake - NetworkObject found", this);
    }


    private void Start()
    {
        // 🟢 Проверяем: это мой игрок или чужой?
        isLocalPlayer = networkObject.IsOwner;
        
        Debug.Log(
            $"[NearbyInteractables] Start - isLocalPlayer={isLocalPlayer}, " +
            $"networkObject.IsOwner={networkObject.IsOwner}",
            this
        );

        // ❌ Если это НЕ мой игрок - отключаем компонент ПОЛНОСТЬЮ
        if (!isLocalPlayer)
        {
            enabled = false;
            Debug.Log("[NearbyInteractables] ⚠️ Disabled for remote player", this);
        }
    }


    /* ================= PUBLIC API ================= */


    /// <summary>
    /// Находит ближайший доступный предмет в поле зрения.
    /// Учитывает расстояние и угол к камере.
    /// </summary>
    public WorldItemNetwork GetBestItem(Camera cam)
    {
        if (cam == null)
            return null;

        // 🟢 Защита: если это не локальный игрок - не работаем
        if (!isLocalPlayer)
            return null;


        // 🟢 ПЕРИОДИЧЕСКАЯ ОЧИСТКА мёртвых объектов
        frameCounter++;
        if (frameCounter >= cleanupInterval)
        {
            CleanupDeadItems();
            frameCounter = 0;
        }


        WorldItemNetwork best = null;
        float bestScore = float.MaxValue;

        Vector3 camPos = cam.transform.position;
        Vector3 camForward = cam.transform.forward;


        // 🟢 Ищем ближайший предмет
        foreach (var item in items)
        {
            // ⚡ Быстрые проверки перед обращением к трансформу
            if (item == null)
                continue;

            if (!item.gameObject.activeSelf)
                continue;

            if (!item.IsPickupAvailable)
                continue;


            // 📐 Расчёты расстояния и угла
            Vector3 toItem = item.transform.position - camPos;
            float distance = toItem.magnitude;
            float angle = Vector3.Angle(camForward, toItem);


            // ❌ Фильтруем по дальности
            if (distance > maxDistance)
                continue;

            // ❌ Фильтруем по углу обзора
            if (angle > maxAngle)
                continue;


            // 🎯 Скоринг: расстояние + угол
            // Ближе = лучше, центральнее = лучше
            float score = distance + angle * 0.03f;
            if (score < bestScore)
            {
                bestScore = score;
                best = item;
            }
        }

        return best;
    }


    /// <summary>
    /// Регистрирует новый предмет в списке.
    /// 
    /// 🟢 Вызывается когда предмет спавнится рядом
    /// ❌ Блокируется для чужих игроков
    /// </summary>
    public void Register(WorldItemNetwork item)
    {
        // 🟢 БЛОКИРУЕМ если это чужой игрок
        if (!isLocalPlayer)
        {
            Debug.LogWarning(
                "[NearbyInteractables] ⚠️ Tried to register on REMOTE player! " +
                $"Item: {item?.name ?? "null"}",
                this
            );
            return;
        }

        if (item == null)
        {
            Debug.LogWarning("[NearbyInteractables] Register called with null item", this);
            return;
        }

        if (!items.Contains(item))
        {
            items.Add(item);
            Debug.Log(
                $"[NearbyInteractables] ✅ Registered: {item.name}, " +
                $"Total items: {items.Count}",
                this
            );
        }
    }


    /// <summary>
    /// Удаляет предмет из списка.
    /// 
    /// 🟢 Вызывается когда предмет подбирается или удаляется
    /// ❌ Блокируется для чужих игроков
    /// </summary>
    public void Unregister(WorldItemNetwork item)
    {
        // 🟢 БЛОКИРУЕМ если это чужой игрок
        if (!isLocalPlayer)
            return;

        if (item == null)
            return;

        if (items.Remove(item))
        {
            Debug.Log(
                $"[NearbyInteractables] ❌ Unregistered: {item.name}, " +
                $"Remaining items: {items.Count}",
                this
            );
        }
    }


    /* ================= PRIVATE HELPERS ================= */


    /// <summary>
    /// Удаляет null и неактивные объекты из списка.
    /// Периодически вызывается из GetBestItem() для оптимизации.
    /// </summary>
    private void CleanupDeadItems()
    {
        int beforeCount = items.Count;
        
        // 🟢 Удаляем null и неактивные объекты
        items.RemoveAll(item => item == null || !item.gameObject.activeSelf);
        
        int afterCount = items.Count;
        int removed = beforeCount - afterCount;

        if (removed > 0)
        {
            Debug.Log(
                $"[NearbyInteractables] 🧹 Cleanup: removed {removed} dead items, " +
                $"{afterCount} remaining",
                this
            );
        }
    }


    /* ================= DEBUG ================= */


#if UNITY_EDITOR
    /// <summary>
    /// Визуализация радиуса поиска в редакторе.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 🎯 Показываем максимальное расстояние (зелёная сфера)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // 📐 Показываем угол обзора (конус)
        // (Примерная визуализация - зависит от камеры)
        Gizmos.color = Color.yellow;
        
        // Рисуем линию вперёд на максимальное расстояние
        Vector3 forward = transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + forward * maxDistance);
    }
#endif


    /// <summary>
    /// Вспомогательный метод для вывода статистики.
    /// Вызывается из PlayerInteractionController или других мест при необходимости.
    /// </summary>
    public void PrintDebugInfo()
    {
        int totalCount = items.Count;
        int nullCount = 0;
        int inactiveCount = 0;
        int activeCount = 0;

        foreach (var item in items)
        {
            if (item == null)
                nullCount++;
            else if (!item.gameObject.activeSelf)
                inactiveCount++;
            else
                activeCount++;
        }

        Debug.Log(
            $"[NearbyInteractables] DEBUG INFO:\n" +
            $"  Total items: {totalCount}\n" +
            $"  Active: {activeCount}\n" +
            $"  Inactive: {inactiveCount}\n" +
            $"  Null: {nullCount}\n" +
            $"  IsLocalPlayer: {isLocalPlayer}\n" +
            $"  Max Distance: {maxDistance}\n" +
            $"  Max Angle: {maxAngle}°",
            this
        );
    }
}