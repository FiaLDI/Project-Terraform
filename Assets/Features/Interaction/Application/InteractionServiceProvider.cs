using Features.Interaction.Application;
using Features.Interaction.Domain;
using UnityEngine;

namespace Features.Interaction.UnityIntegration
{
    public static class InteractionServiceProvider
    {
        public static InteractionRayService Ray { get; private set; }
        public static event System.Action<InteractionRayService> OnRayInitialized;

        public static void Init(IInteractionRayProvider provider)
        {
            if (Ray != null)
                return;

            int interactableMask = LayerMask.GetMask("Interactable");
            int ignoreMask = LayerMask.GetMask("Player");

            Ray = new InteractionRayService(
                provider,
                interactableMask,
                ignoreMask
            );

            OnRayInitialized?.Invoke(Ray);
        }
    }
}
