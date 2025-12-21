using System;
using System.Collections.Generic;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Abilities.Application
{
    public class AbilityService
    {
        private readonly object _owner;

        private IEnergyStats _energy;
        private AbilityExecutor _executor;
        private LayerMask _groundMask;

        // Camera services (LOCAL ONLY)
        private readonly ICameraControlService _control;
        private readonly ICameraRuntimeService _runtime;

        private readonly Dictionary<AbilitySO, float> _cooldowns = new();

        public event Action<AbilitySO, float, float> OnCooldownChanged;
        public event Action<AbilitySO> OnAbilityCast;
        public event Action<AbilitySO> OnChannelStarted;
        public event Action<AbilitySO, float, float> OnChannelProgress;
        public event Action<AbilitySO> OnChannelCompleted;
        public event Action<AbilitySO> OnChannelInterrupted;

        private bool _isChanneling;
        private AbilitySO _channelAbility;
        private float _channelTimer;
        private float _channelDuration;
        private AbilityContext _channelContext;

        public bool IsChanneling => _isChanneling;
        public AbilitySO CurrentChannelAbility => _channelAbility;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================
        public AbilityService(
            object owner,
            IEnergyStats energy,
            LayerMask groundMask,
            AbilityExecutor executor
        )
        {
            _owner = owner;
            _energy = energy;
            _groundMask = groundMask;
            _executor = executor;

            _control = CameraServiceProvider.Control;
            _runtime = CameraServiceProvider.Runtime;

            if (_control == null)
                Debug.LogWarning("[AbilityService] CameraControlService not ready yet");

            if (_runtime == null)
                Debug.LogWarning("[AbilityService] CameraRuntimeService not ready yet");
        }

        public void SetEnergy(IEnergyStats e) => _energy = e;
        public void SetExecutor(AbilityExecutor e) => _executor = e;
        public void SetGroundMask(LayerMask m) => _groundMask = m;

        // ============================================================
        // MAIN LOOP
        // ============================================================
        public void Tick(float deltaTime)
        {
            UpdateCooldowns(deltaTime);
            UpdateChannel(deltaTime);
        }

        private void UpdateCooldowns(float dt)
        {
            if (_cooldowns.Count == 0) return;

            var keys = new List<AbilitySO>(_cooldowns.Keys);
            foreach (var ab in keys)
            {
                float t = Mathf.Max(0f, _cooldowns[ab] - dt);
                _cooldowns[ab] = t;
                OnCooldownChanged?.Invoke(ab, t, ab.cooldown);
            }
        }

        private void UpdateChannel(float dt)
        {
            if (!_isChanneling || _channelAbility == null)
                return;

            _channelTimer += dt;
            OnChannelProgress?.Invoke(_channelAbility, _channelTimer, _channelDuration);

            if (_channelTimer >= _channelDuration)
                CompleteChannel();
        }

        // ============================================================
        // CAST
        // ============================================================
        public bool TryCast(AbilitySO ability, int slot)
        {
            if (ability == null) return false;
            if (_isChanneling) return false;
            if (_executor == null) return false;
            if (_energy == null) return false;

            if (GetCooldownRemaining(ability) > 0f)
                return false;

            float finalCost = ability.energyCost * _energy.CostMultiplier;
            if (!_energy.TrySpend(finalCost))
                return false;

            AbilityContext ctx = BuildContext(slot);

            if (ability.castType == AbilityCastType.Instant)
            {
                OnAbilityCast?.Invoke(ability);
                _executor.Execute(ability, ctx);
                _cooldowns[ability] = ability.cooldown;
                return true;
            }

            if (ability.castType == AbilityCastType.Channel)
            {
                StartChannel(ability, ctx);
                return true;
            }

            return false;
        }

        // ============================================================
        // CHANNEL
        // ============================================================
        private void StartChannel(AbilitySO ability, AbilityContext ctx)
        {
            _isChanneling = true;
            _channelAbility = ability;
            _channelContext = ctx;
            _channelDuration = Mathf.Max(0.01f, ability.castTime);
            _channelTimer = 0f;

            OnChannelStarted?.Invoke(ability);
        }

        private void CompleteChannel()
        {
            AbilitySO ab = _channelAbility;
            AbilityContext ctx = _channelContext;

            _isChanneling = false;
            _channelAbility = null;

            _executor.Execute(ab, ctx);
            OnChannelCompleted?.Invoke(ab);
            _cooldowns[ab] = ab.cooldown;
        }

        public void InterruptChannel()
        {
            if (!_isChanneling)
                return;

            AbilitySO ab = _channelAbility;
            _isChanneling = false;
            _channelAbility = null;

            OnChannelInterrupted?.Invoke(ab);
        }

        // ============================================================
        // CONTEXT BUILDING (SAFE)
        // ============================================================
        private AbilityContext BuildContext(int slot)
        {
            float yaw = _control?.State.Yaw ?? 0f;
            float pitch = _control?.State.Pitch ?? 0f;

            Vector3 direction = BuildDirection(yaw, pitch);
            Vector3 targetPoint = BuildTargetPoint(direction);

            return new AbilityContext(
                owner: _owner,
                targetPoint: targetPoint,
                direction: direction,
                slotIndex: slot,
                yaw: yaw,
                pitch: pitch
            );
        }

        private Vector3 BuildDirection(float yaw, float pitch)
        {
            float yawRad = Mathf.Deg2Rad * yaw;
            float pitchRad = Mathf.Deg2Rad * pitch;

            return new Vector3(
                Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
                Mathf.Sin(pitchRad),
                Mathf.Cos(pitchRad) * Mathf.Cos(yawRad)
            ).normalized;
        }

        private Vector3 BuildTargetPoint(Vector3 direction)
        {
            UnityEngine.Camera cam = _runtime?.CurrentCamera;

            if (cam == null)
                return direction * 10f;

            Vector3 origin = cam.transform.position;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, _groundMask))
                return hit.point;

            return origin + direction * 30f;
        }

        // ============================================================
        // UTILS
        // ============================================================
        public float GetCooldownRemaining(AbilitySO ab)
        {
            return _cooldowns.TryGetValue(ab, out float t) ? t : 0f;
        }
    }
}
