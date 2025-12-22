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

            Debug.Log("[INTERACTION] Resolver created");
        }

        public InteractionTarget Resolve(UnityEngine.Camera cam)
        {
            // 1️⃣ PICKUP
            if (nearby != null && cam != null)
            {
                var best = nearby.GetBestItem(cam);
                if (best != null)
                {
                    Debug.Log("[INTERACTION] Resolved PICKUP");
                    return InteractionTarget.ForPickup(best);
                }
            }

            // 2️⃣ INTERACTABLE
            if (ray != null)
            {
                var hit = ray.Raycast();
                if (hit.Hit)
                {
                    Debug.Log($"[INTERACTION] Ray hit: {hit.HitInfo.collider.name}");

                    if (interactionService.TryGetInteractable(hit, out var interactable))
                    {
                        Debug.Log("[INTERACTION] Resolved INTERACTABLE");
                        return InteractionTarget.ForInteractable(interactable);
                    }
                }
            }

            return InteractionTarget.None;
        }
    }
}
