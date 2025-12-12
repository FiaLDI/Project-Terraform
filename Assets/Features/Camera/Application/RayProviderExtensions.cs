using Features.Interaction.Domain;

public static class RayProviderExtensions
{
    public static bool IsValid(this IInteractionRayProvider provider)
    {
        return provider != null &&
               provider.MaxDistance > 0;
    }
}
