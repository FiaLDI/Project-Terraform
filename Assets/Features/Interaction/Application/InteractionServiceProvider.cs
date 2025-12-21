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

            Ray = new InteractionRayService(
                provider,
                ~LayerMask.GetMask("Player"),
                LayerMask.GetMask("Player")
            );

            OnRayInitialized?.Invoke(Ray);
        }
    }

}
