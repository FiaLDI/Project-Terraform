using Features.Interaction.Domain;
using UnityEngine;

namespace Features.Interaction.Application
{
    public class InteractionService
    {
        public bool TryGetInteractable(InteractionRayHit hit, out IInteractable interactable)
        {
            interactable = null;

            if (!hit.Hit)
                return false;

            var col = hit.HitInfo.collider;

            interactable = col.GetComponentInParent<IInteractable>();

            return interactable != null;
        }

    }
}
