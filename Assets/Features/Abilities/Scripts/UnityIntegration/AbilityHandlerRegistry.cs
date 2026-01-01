using System.Collections.Generic;
using Features.Abilities.Domain;

namespace Features.Abilities.UnityIntegration
{
    public static class AbilityHandlerRegistry
    {
        private static readonly List<IAbilityHandler> handlers = new();

        public static IReadOnlyList<IAbilityHandler> All => handlers;

        internal static void Register(IAbilityHandler handler)
        {
            handlers.Add(handler);
        }
    }
}
