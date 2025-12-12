using Features.Interaction.Domain;

namespace Features.Interaction.Application
{
    public class InteractionService
    {
        public bool TryGetInteractable(InteractionRayHit hit, out IInteractable interactable)
        {
            interactable = null;
            if (!hit.Hit) return false;

            interactable = hit.HitInfo.collider.GetComponentInParent<IInteractable>();
            return interactable != null;
        }
    }
}
