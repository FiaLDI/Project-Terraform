using UnityEngine;
using Features.Player;
using Features.Items.Domain;
using Features.Items.Data;
using Features.Interaction.Domain;

public class NearbyItemPresenter : MonoBehaviour
{
    public ItemInstance instance;

    private INearbyInteractables nearby;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    private void OnEnable()
    {
        nearby = LocalPlayerContext.Get<NearbyInteractables>();

        if (nearby == null)
        {
            Debug.LogWarning(
                $"[NearbyItemPresenter] NearbyInteractables not found for {name}"
            );
            return;
        }

        nearby.Register(this);
    }

    private void OnDisable()
    {
        nearby?.Unregister(this);
        nearby = null;
    }

    // ======================================================
    // INIT
    // ======================================================

    public void Initialize(Item item, int quantity)
    {
        instance = new ItemInstance(item, quantity);
    }
}
