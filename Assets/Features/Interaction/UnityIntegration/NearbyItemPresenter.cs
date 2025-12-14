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

        Debug.Log(
            $"[NearbyItemPresenter][Awake] {name} | holder={(holder ? "OK" : "NULL")}",
            this
        );

        if (holder == null)
            Debug.LogError("[NearbyItemPresenter] ItemRuntimeHolder MISSING", this);
    }

    private void OnEnable()
    {
        Debug.Log($"[NearbyItemPresenter][OnEnable] {name}", this);

        nearby = LocalPlayerContext.Get<NearbyInteractables>();

        Debug.Log(
            $"[NearbyItemPresenter] NearbyInteractables = {(nearby == null ? "NULL" : "OK")}",
            this
        );

        if (nearby == null)
        {
            Debug.LogWarning("[NearbyItemPresenter] Register FAILED — nearby is NULL", this);
            return;
        }

        nearby.Register(this);
        Debug.Log("[NearbyItemPresenter] Registered");
    }

    private void OnDisable()
    {
        Debug.Log($"[NearbyItemPresenter][OnDisable] {name}", this);

        if (nearby != null)
        {
            nearby.Unregister(this);
            Debug.Log("[NearbyItemPresenter] Unregistered");
        }

        nearby = null;
    }

    public ItemInstance GetInstance()
    {
        var inst = holder != null ? holder.Instance : null;

        if (inst == null)
            Debug.LogWarning("[NearbyItemPresenter] ItemInstance is NULL", this);

        return inst;
    }
}
