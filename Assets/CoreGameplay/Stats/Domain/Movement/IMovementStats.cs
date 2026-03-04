using Features.Stats.Domain;

namespace Features.Stats.Domain
{
    public interface IMovementStats
    {
        float BaseSpeed { get; }
        float WalkSpeed { get; }
        float SprintSpeed { get; }
        float CrouchSpeed { get; }
        float RotationSpeed { get; }

        float Gravity { get; }
        float JumpHeight { get; }

        void ApplyBase(
            float baseSpeed,
            float walk,
            float sprint,
            float crouch,
            float rotation,
            float gravity,
            float jumpHeight);

        bool TryAdd(StatKey key, float value);
        bool TryMultiply(StatKey key, float value);

        void Reset();
    }
}
