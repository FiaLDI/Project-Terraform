using Features.Items.Domain;
using UnityEngine;

public class ItemModeSwitch : MonoBehaviour, IItemModeSwitch
{
    [Header("World Mode")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider physicalCollider;
    [SerializeField] private Collider pickupTrigger;
    [SerializeField] private MonoBehaviour nearbyItemPresenter;

    [Header("Equipped Mode")]
    [SerializeField] private MonoBehaviour[] equippedOnlyBehaviours;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        var colliders = GetComponents<Collider>();
        if (colliders.Length > 0)
            physicalCollider = colliders[0];
    }

    // ===============================
    // WORLD
    // ===============================

    public void SetWorldMode()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (physicalCollider != null)
            physicalCollider.enabled = true;

        if (pickupTrigger != null)
            pickupTrigger.enabled = true;

        if (nearbyItemPresenter != null)
            nearbyItemPresenter.enabled = true;

        SetEquippedBehaviours(false);
    }

    // ===============================
    // EQUIPPED
    // ===============================

    public void SetEquippedMode()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (physicalCollider != null)
            physicalCollider.enabled = false;

        if (pickupTrigger != null)
            pickupTrigger.enabled = false;

        if (nearbyItemPresenter != null)
            nearbyItemPresenter.enabled = false;

        SetEquippedBehaviours(true);
    }

    // ===============================

    private void SetEquippedBehaviours(bool enabled)
    {
        if (equippedOnlyBehaviours == null)
            return;

        foreach (var b in equippedOnlyBehaviours)
        {
            if (b != null)
                b.enabled = enabled;
        }
    }
}
