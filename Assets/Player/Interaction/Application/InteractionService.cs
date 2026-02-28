using Features.Interaction.Domain;
using UnityEngine;

namespace Features.Interaction.Application
{
    public class InteractionService
    {
        public bool TryGetInteractable(InteractionRayHit hit, out IInteractable interactable)
        {
            interactable = null;
            

            if (!hit.Hit || hit.HitInfo.collider == null)
                return false;

            interactable = hit.HitInfo.collider.GetComponent<IInteractable>();

            if (interactable == null)
                interactable = hit.HitInfo.collider.GetComponentInParent<IInteractable>();

            return interactable != null;
        }
    }
}
