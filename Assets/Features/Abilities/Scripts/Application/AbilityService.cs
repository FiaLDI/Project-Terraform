using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Energy.Application;
using Features.Energy.Domain;

namespace Features.Abilities.Application
{
    public class AbilityService
    {
        private readonly GameObject _owner;
        private Camera _aimCamera;
        private IEnergy _energy;
        private LayerMask _groundMask;
        private AbilityExecutor _executor;

        private readonly EnergyCostService _costService;

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

        public AbilityService(
            GameObject owner,
            Camera aimCamera,
            IEnergy energy,
            LayerMask groundMask,
            AbilityExecutor executor,
            EnergyCostService costService
        )
        {
            _owner      = owner;
            _aimCamera  = aimCamera;
            _energy     = energy;
            _groundMask = groundMask;
            _executor   = executor;
            _costService = costService;
        }

        // External overrides
        public void SetCamera(Camera cam)         => _aimCamera = cam;
        public void SetEnergy(IEnergy e)         => _energy = e;
        public void SetExecutor(AbilityExecutor e)=> _executor = e;
        public void SetGroundMask(LayerMask m)    => _groundMask = m;

        // ========================= TICK =========================

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
                float t = _cooldowns[ab] - dt;
                if (t < 0f) t = 0f;

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

        // ========================= CAST =========================

        public bool TryCast(AbilitySO ability, int slot)
        {
            if (ability == null) 
                return false;

            if (_isChanneling)
                return false;

            if (_executor == null)
                return false;

            if (_energy == null)
            {
                Debug.LogError("[AbilityService] No IEnergy provided!");
                return false;
            }

            // Cooldown check
            if (GetCooldownRemaining(ability) > 0f)
                return false;

            // ================================
            // COST → через EnergyStatsAdapter + EnergyCostService
            // ================================
            float finalCost = _costService.ApplyModifiers(ability.energyCost);

            if (!_energy.HasEnergy(finalCost))
                return false;

            if (!_energy.TrySpend(finalCost))
                return false;

            var ctx = BuildContext(ability, slot);

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

        // ========================= CHANNEL =========================

        private void StartChannel(AbilitySO ability, AbilityContext ctx)
        {
            _isChanneling   = true;
            _channelAbility = ability;
            _channelContext = ctx;
            _channelDuration = Mathf.Max(0.01f, ability.castTime);
            _channelTimer    = 0f;

            OnChannelStarted?.Invoke(ability);
        }

        private void CompleteChannel()
        {
            var ab  = _channelAbility;
            var ctx = _channelContext;

            _isChanneling   = false;
            _channelAbility = null;

            _executor.Execute(ab, ctx);
            OnChannelCompleted?.Invoke(ab);

            _cooldowns[ab] = ab.cooldown;
        }

        // ========================= CONTEXT =========================

        public float GetCooldownRemaining(AbilitySO ab)
        {
            if (_cooldowns.TryGetValue(ab, out var t))
                return Mathf.Max(0f, t);
            return 0f;
        }

        private AbilityContext BuildContext(AbilitySO ab, int slot)
        {
            Vector3 targetPoint = GetTargetPoint();
            Vector3 dir = _aimCamera != null
                ? _aimCamera.transform.forward
                : _owner.transform.forward;

            return new AbilityContext(
                owner: _owner,
                aimCamera: _aimCamera,
                targetPoint: targetPoint,
                direction: dir,
                slotIndex: slot
            );
        }

        private Vector3 GetTargetPoint()
        {
            if (_aimCamera == null || _groundMask == 0)
                return _owner.transform.position + _owner.transform.forward * 3f;

            Ray ray = _aimCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));

            return Physics.Raycast(ray, out var hit, 100f, _groundMask)
                ? hit.point
                : _owner.transform.position + _owner.transform.forward * 3f;
        }
    }
}
