using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface IMovementStats
    {
        float BaseSpeed { get; }
        float WalkSpeed { get; }
        float SprintSpeed { get; }
        float CrouchSpeed { get; }

        void ApplyBase(float baseSpeed, float walk, float sprint, float crouch);
        void ApplyBuff(BuffSO cfg, bool apply);
    }
}
