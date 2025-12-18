using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class MovementStatsAdapter : MonoBehaviour
    {
        private IMovementStats _stats;

        public float BaseSpeed => _stats.BaseSpeed;
        public float WalkSpeed => _stats.WalkSpeed;
        public float SprintSpeed => _stats.SprintSpeed;
        public float CrouchSpeed => _stats.CrouchSpeed;

        public void Init(IMovementStats stats)
        {
            _stats = stats;
        }

        public float GetSpeed(bool sprint, bool crouch)
        {
            if (crouch) return CrouchSpeed;
            if (sprint) return SprintSpeed;
            return WalkSpeed;
        }
    }
}
