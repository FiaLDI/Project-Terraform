using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Stats.Domain;

namespace Features.Abilities.Application
{
    public class AbilityService
    {
        private readonly GameObject _owner;
        private Camera _aimCamera;

        private IEnergyStats _energy;

        private LayerMask _groundMask;
        private AbilityExecutor _executor;

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
            GameObject owner,
            Camera aimCamera,
            IEnergyStats energy,   
            LayerMask groundMask,
            AbilityExecutor executor
        )
        {
            _owner = owner;
            _aimCamera = aimCamera;
            _energy = energy;
            _groundMask = groundMask;
            _executor = executor;
        }

        // External overrides
        public void SetCamera(Camera cam) => _aimCamera = cam;
        public void SetEnergy(IEnergyStats e) => _energy = e;   // <<==== FIX
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

            if (_energy == null)
            {
                Debug.LogError("[AbilityService] No IEnergyStats assigned!");
                return false;
            }

            // ��� Cooldown ���
            if (GetCooldownRemaining(ability) > 0f)
                return false;

            // ��� COST ���
            float baseCost = ability.energyCost;
            float finalCost = baseCost * _energy.CostMultiplier; 

            if (!_energy.HasEnergy(finalCost))
                return false;

            if (!_energy.TrySpend(finalCost))
                return false;

            var ctx = BuildContext(ability, slot);

            // ��� Instant ���
            if (ability.castType == AbilityCastType.Instant)
            {
                OnAbilityCast?.Invoke(ability);
                _executor.Execute(ability, ctx);
                _cooldowns[ability] = ability.cooldown;
                return true;
            }

            // ��� Channel ���
            if (ability.castType == AbilityCastType.Channel)
            {
                StartChannel(ability, ctx);
                return true;
            }

            return false;
        }



        // ============================================================
        // CHANNEL LOGIC
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
            var ab = _channelAbility;
            var ctx = _channelContext;

            _isChanneling = false;
            _channelAbility = null;

            _executor.Execute(ab, ctx);
            OnChannelCompleted?.Invoke(ab);

            _cooldowns[ab] = ab.cooldown;
        }



        // ============================================================
        // UTILS
        // ============================================================
        public float GetCooldownRemaining(AbilitySO ab)
        {
            return _cooldowns.TryGetValue(ab, out var t) ? t : 0f;
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
