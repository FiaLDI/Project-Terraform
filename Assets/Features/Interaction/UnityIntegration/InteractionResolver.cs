using UnityEngine;
using Features.Interaction.Domain;
using Features.Interaction.Application;

namespace Features.Interaction.UnityIntegration
{
    public sealed class InteractionResolver
    {
        private readonly InteractionRayService ray;
        private readonly INearbyInteractables nearby;
        private readonly InteractionService interactionService = new();

        public InteractionResolver(
            InteractionRayService ray,
            INearbyInteractables nearby)
        {
            this.ray = ray;
            this.nearby = nearby;

        }

        public InteractionTarget Resolve(UnityEngine.Camera cam)
        {
            // 1️⃣ PICKUP
            if (nearby != null && cam != null)
            {
                Debug.Log($"[Resolver] Nearby has {nearby} items"); // сделай Count в INearbyInteractables

                var best = nearby.GetBestItem(cam);
                Debug.Log($"[Resolver] GetBestItem result = {best}", best);

                if (best != null)
                    return InteractionTarget.ForPickup(best);
            }

            // 2️⃣ INTERACTABLE
            if (ray != null)
            {
                var hit = ray.Raycast();
                if (hit.Hit)
                {

                    if (interactionService.TryGetInteractable(hit, out var interactable))
                    {
                        return InteractionTarget.ForInteractable(interactable);
                    }
                }
            }

            return InteractionTarget.None;
        }
    }
}
