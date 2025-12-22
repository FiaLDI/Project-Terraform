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
        public NearbyItemPresenter Pickup;
        public IInteractable Interactable;

        public static InteractionTarget None =>
            new InteractionTarget { Type = InteractionTargetType.None };

        public static InteractionTarget ForPickup(NearbyItemPresenter pickup) =>
            new InteractionTarget
            {
                Type = InteractionTargetType.Pickup,
                Pickup = pickup
            };

        public static InteractionTarget ForInteractable(IInteractable interactable) =>
            new InteractionTarget
            {
                Type = InteractionTargetType.Interactable,
                Interactable = interactable
            };
    }
}
