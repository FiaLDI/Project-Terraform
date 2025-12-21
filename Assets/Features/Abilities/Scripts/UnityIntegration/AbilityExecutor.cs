using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;

namespace Features.Abilities.UnityIntegration
{
    public class AbilityExecutor : MonoBehaviour
    {
        public static AbilityExecutor I { get; private set; }

        private readonly Dictionary<Type, IAbilityHandler> _handlers = new();

        private void Awake()
        {
            I = this;

            Register(new DeployTurretHandler());
            Register(new ChargeDeviceHandler());
            Register(new OverloadPulseHandler());
            Register(new RepairDroneHandler());
            Register(new ShieldGridHandler());
        }

        private void Register(IAbilityHandler handler)
        {
            var type = handler.AbilityType;

            if (_handlers.ContainsKey(type))
            {
                Debug.LogWarning($"[AbilityExecutor] Handler already registered for {type.Name}");
                return;
            }

            _handlers.Add(type, handler);
        }

        public void Execute(AbilitySO ability, AbilityContext ctx)
        {
            if (ability == null)
            {
                Debug.LogWarning("[AbilityExecutor] Ability is null");
                return;
            }

            if (_handlers.TryGetValue(ability.GetType(), out var handler))
            {
                handler.Execute(ability, ctx);
            }
            else
            {
                Debug.LogError("[AbilityExecutor] No handler found for " + ability.GetType());
            }
        }
    }
}
