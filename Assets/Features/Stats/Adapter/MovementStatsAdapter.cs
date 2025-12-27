using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class MovementStatsAdapter : MonoBehaviour
    {
        private IMovementStats _stats;

        public float BaseSpeed => _stats != null ? _stats.BaseSpeed : 0f;
        public float WalkSpeed => _stats != null ? _stats.WalkSpeed : 0f;
        public float SprintSpeed => _stats != null ? _stats.SprintSpeed : 0f;
        public float CrouchSpeed => _stats != null ? _stats.CrouchSpeed : 0f;
        
        public bool IsReady => _stats != null;

        public void Init(IMovementStats stats)
        {
            _stats = stats;
        }

        public float GetSpeed(bool sprint, bool crouch)
        {
            if (!IsReady) return 0f;

            if (crouch) return CrouchSpeed;
            if (sprint) return SprintSpeed;
            return WalkSpeed;
        }
    }
}
