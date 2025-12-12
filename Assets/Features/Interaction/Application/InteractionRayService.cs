using UnityEngine;
using Features.Interaction.Domain;

namespace Features.Interaction.Application
{
    public class InteractionRayService
    {
        private readonly IInteractionRayProvider provider;
        private readonly LayerMask includeMask;
        private readonly LayerMask ignoreMask;

        public InteractionRayService(
            IInteractionRayProvider provider,
            LayerMask includeMask,
            LayerMask ignoreMask)
        {
            this.provider = provider;
            this.includeMask = includeMask;
            this.ignoreMask = ignoreMask;
        }

        public IInteractionRayProvider Provider => provider;

        public InteractionRayHit Raycast()
        {
            Ray ray = provider.GetRay();
            int mask = includeMask & ~ignoreMask;

            if (Physics.Raycast(ray, out var hit, provider.MaxDistance, mask))
                return new InteractionRayHit(true, hit);

            return new InteractionRayHit(false, default);
        }
    }
}
