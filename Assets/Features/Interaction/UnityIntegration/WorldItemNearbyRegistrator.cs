using UnityEngine;
using Features.Items.UnityIntegration;
using Features.Interaction.UnityIntegration;

[RequireComponent(typeof(WorldItemNetwork))]
[RequireComponent(typeof(Collider))]
public sealed class WorldItemNearbyRegistrator : MonoBehaviour
{
    private WorldItemNetwork item;

    private void Awake()
    {
        item = GetComponent<WorldItemNetwork>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var nearby = other.GetComponentInParent<NearbyInteractables>();
        if (nearby == null)
            return;

        nearby.Register(item);
    }

    private void OnTriggerExit(Collider other)
    {
        var nearby = other.GetComponentInParent<NearbyInteractables>();
        if (nearby == null)
            return;

        nearby.Unregister(item);
    }
}
