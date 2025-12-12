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

        public InteractionRayHit Raycast()
        {
            try
            {
                Ray ray = provider.GetRay();

                bool hit = Physics.Raycast(
                    ray,
                    out var hitInfo,
                    provider.MaxDistance,
                    includeMask,
                    QueryTriggerInteraction.Ignore
                );

                return new InteractionRayHit(hit, hitInfo);
            }
            catch
            {
                return default;
            }
        }

    }
}
