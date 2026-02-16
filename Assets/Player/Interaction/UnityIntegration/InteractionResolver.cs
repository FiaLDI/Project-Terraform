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
            // 🔒 защита от уничтоженного NearbyInteractables
            if (nearby is UnityEngine.Object o && o == null)
                return InteractionTarget.None;

            // 1️⃣ PICKUP
            if (nearby != null && cam != null)
            {
                var best = nearby.GetBestItem(cam);
                if (best != null)
                    return InteractionTarget.ForPickup(best);
            }

            // 2️⃣ INTERACTABLE
            if (ray != null)
            {
                var hit = ray.Raycast();
                if (hit.Hit &&
                    interactionService.TryGetInteractable(hit, out var interactable))
                {
                    return InteractionTarget.ForInteractable(interactable);
                }
            }

            return InteractionTarget.None;
        }
    }
}
