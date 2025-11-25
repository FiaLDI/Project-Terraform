using UnityEngine;

public class ItemAutoRegister : MonoBehaviour
{
    private ItemObject io;

    private void Start()
    {
        io = GetComponent<ItemObject>();

        // если ItemAutoRegister висит на объекте без ItemObject, то сразу выключаем его
        if (io == null)
        {
            Debug.LogWarning($"ItemAutoRegister висит на объекте {gameObject.name}, но ItemObject не найден!");
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // если предмет не worldObject он в руках НЕ регистрируем
        if (!io.isWorldObject) return;

        NearbyInteractables.instance.Register(io);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // также проверяем, чтобы случайно не удалять из списка предмет, который сейчас в руках
        if (!io.isWorldObject) return;

        NearbyInteractables.instance.Unregister(io);
    }
}
