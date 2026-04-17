using UnityEngine;
using Features.Stats.Domain;
using FishNet.Object;

namespace Features.Stats.Adapter
{
    public class MovementStatsAdapter : MonoBehaviour
    {
        private IMovementStats _stats;

        // ===== CLIENT CACHE =====
        private float _walk;
        private float _sprint;
        private float _crouch;
        private float _rotation;
        private float _gravity;
        private float _jump;

        private bool _isReady;
        private NetworkObject networkObject;
        private float nextDebugLogTime;

        [Header("Debug")]
        [SerializeField] private bool debugMovementStats;
        [SerializeField] private bool debugLogGetSpeed;
        [SerializeField] private float debugLogInterval = 0.5f;

        public bool IsReady => _stats != null || _isReady;

        // ===== READ API =====

        public float WalkSpeed => _stats != null ? _stats.WalkSpeed : _walk;
        public float SprintSpeed => _stats != null ? _stats.SprintSpeed : _sprint;
        public float CrouchSpeed => _stats != null ? _stats.CrouchSpeed : _crouch;
        public float RotationSpeed => _stats != null ? _stats.RotationSpeed : _rotation;

        public float Gravity => _stats != null ? _stats.Gravity : _gravity;
        public float JumpHeight => _stats != null ? _stats.JumpHeight : _jump;

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
        }

        // ===== SERVER INIT =====

        public void Init(IMovementStats stats)
        {
            _stats = stats;
            MaybeLogSnapshot("INIT");
        }

        // ===== CLIENT SYNC =====

        public void Set(
            float walk,
            float sprint,
            float crouch,
            float rotation,
            float gravity,
            float jumpHeight)
        {
            _walk = walk;
            _sprint = sprint;
            _crouch = crouch;
            _rotation = rotation;

            _gravity = gravity;
            _jump = jumpHeight;

            _isReady = true;
            MaybeLogSnapshot("SET");
        }

        // ===== SPEED HELPER =====

        public float GetSpeed(bool sprint, bool crouch)
        {
            if (!IsReady)
            {
                MaybeLogGetSpeed(sprint, crouch, 0f, "not-ready");
                return 0f;
            }

            float speed = WalkSpeed;
            string reason = "walk";

            if (crouch)
            {
                speed = CrouchSpeed;
                reason = "crouch";
            }
            else if (sprint)
            {
                speed = SprintSpeed;
                reason = "sprint";
            }

            MaybeLogGetSpeed(sprint, crouch, speed, reason);
            return speed;
        }

        private void MaybeLogSnapshot(string source)
        {
            if (!debugMovementStats || !ShouldLog())
                return;

            Debug.Log(
                $"[MoveStats][{source}] {name} role={GetRoleLabel()} ready={IsReady} " +
                $"walk={WalkSpeed:0.##} sprint={SprintSpeed:0.##} crouch={CrouchSpeed:0.##} " +
                $"rotation={RotationSpeed:0.##} gravity={Gravity:0.##} jump={JumpHeight:0.##}",
                this);
        }

        private void MaybeLogGetSpeed(bool sprint, bool crouch, float speed, string reason)
        {
            if (!debugMovementStats || !debugLogGetSpeed || !ShouldLog())
                return;

            Debug.Log(
                $"[MoveStats][GET] {name} role={GetRoleLabel()} ready={IsReady} " +
                $"input(sprint={sprint}, crouch={crouch}) => {reason} speed={speed:0.##} " +
                $"walk={WalkSpeed:0.##} sprintSpeed={SprintSpeed:0.##} crouchSpeed={CrouchSpeed:0.##}",
                this);
        }

        private bool ShouldLog()
        {
            float now = Time.unscaledTime;
            if (now < nextDebugLogTime)
                return false;

            nextDebugLogTime = now + Mathf.Max(0.1f, debugLogInterval);
            return true;
        }

        private string GetRoleLabel()
        {
            if (networkObject == null)
                return "no-net";

            if (networkObject.IsServerStarted && networkObject.IsOwner)
                return "host-owner";

            if (networkObject.IsServerStarted)
                return "server";

            if (networkObject.IsOwner)
                return "owner-client";

            return "remote-client";
        }
    }
}
