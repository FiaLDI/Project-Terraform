using UnityEngine;
using Features.Player;
using Features.Interaction.Domain;
using Features.Items.UnityIntegration;
using Features.Items.Domain;

public class NearbyItemPresenter : MonoBehaviour
{
    private INearbyInteractables nearby;
    private ItemRuntimeHolder holder;

    private void Awake()
    {
        holder = GetComponent<ItemRuntimeHolder>();
        if (holder == null)
        {
            Debug.LogError($"[NearbyItemPresenter] ItemRuntimeHolder missing on {name}", this);
        }
    }

    private void OnEnable()
    {
        nearby = LocalPlayerContext.Get<NearbyInteractables>();
         Debug.Log(
            $"[NearbyItemPresenter] nearby={(nearby == null ? "NULL" : "OK")}",
            this
    );
        if (nearby == null)
        {
            Debug.LogWarning("[NearbyItemPresenter] NearbyInteractables NOT FOUND");
            return;
        }

        nearby.Register(this);
    }

    private void OnDisable()
    {
        nearby?.Unregister(this);
        nearby = null;
    }

    public ItemInstance GetInstance()
    {
        return holder != null ? holder.Instance : null;
    }
}
