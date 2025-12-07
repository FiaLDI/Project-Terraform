using Features.Interaction.Application;
using Features.Interaction.Domain;
using UnityEngine;

namespace Features.Interaction.UnityIntegration
{
    public static class InteractionServiceProvider
    {
        public static InteractionRayService Ray { get; private set; }

        public static void Init(IInteractionRayProvider provider)
        {
            // Один единый луч для ВСЕХ систем
            Ray = new InteractionRayService(
                provider,
                ~LayerMask.GetMask("Player"), // всё, кроме игрока
                LayerMask.GetMask("Player")
            );
        }
    }
}
