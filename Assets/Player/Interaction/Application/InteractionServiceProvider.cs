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
            if (provider == null)
                return;

            // 🔥 если Ray уже есть — проверяем, жив ли provider
            if (Ray != null)
            {
                if (RayProviderDestroyed())
                {
                    Ray = null;
                }
                else
                {
                    return;
                }
            }

            int interactableMask = LayerMask.GetMask("Interactable");
            int ignoreMask       = LayerMask.GetMask("Player");

            Ray = new InteractionRayService(
                provider,
                interactableMask,
                ignoreMask
            );

            OnRayInitialized?.Invoke(Ray);
        }

        private static bool RayProviderDestroyed()
        {
            // получаем provider через рефлексию (минимальный хак без переписывания архитектуры)
            var field = typeof(InteractionRayService)
                .GetField("provider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field == null || Ray == null)
                return true;

            var provider = field.GetValue(Ray);

            if (provider is UnityEngine.Object uo)
                return uo == null;

            return provider == null;
        }

        public static void Reset()
        {
            Ray = null;
        }
    }
}