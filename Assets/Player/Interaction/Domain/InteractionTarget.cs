using Features.Items.UnityIntegration; // где лежит WorldItemNetwork

namespace Features.Interaction.Domain
{
    public enum InteractionTargetType
    {
        None,
        Pickup,
        Interactable
    }

    public struct InteractionTarget
    {
        public InteractionTargetType Type;
        public WorldItemNetwork WorldItem;
        public IInteractable Interactable;

        public static InteractionTarget None =>
            new InteractionTarget { Type = InteractionTargetType.None };

        public static InteractionTarget ForPickup(WorldItemNetwork worldItem) =>
            new InteractionTarget
            {
                Type = InteractionTargetType.Pickup,
                WorldItem = worldItem
            };

        public static InteractionTarget ForInteractable(IInteractable interactable) =>
            new InteractionTarget
            {
                Type = InteractionTargetType.Interactable,
                Interactable = interactable
            };
    }
}
