using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface IMovementStats
    {
        float BaseSpeed { get; }
        float WalkSpeed { get; }
        float SprintSpeed { get; }
        float CrouchSpeed { get; }
        public float RotationSpeed { get; }

        void ApplyBase(float baseSpeed, float walk, float sprint, float crouch, float rotation);
        void ApplyBuff(BuffSO cfg, bool apply);
    }
}
