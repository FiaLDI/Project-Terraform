using UnityEngine;

public class ItemAutoRegister : MonoBehaviour
{
    private ItemObject io;

    private void Start()
    {
        io = GetComponent<ItemObject>();

        if (io == null)
        {
            Debug.LogWarning($"ItemAutoRegister висит на объекте {gameObject.name}, но ItemObject не найден!");
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!io.isWorldObject) return;

        NearbyInteractables.instance.Register(io);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!io.isWorldObject) return;

        NearbyInteractables.instance.Unregister(io);
    }
}
