// Assets/Features/Abilities/Scripts/UnityIntegration/AbilityExecutor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Features.Abilities.Domain;
using FishNet;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    public sealed class AbilityExecutor : MonoBehaviour
    {
        public static AbilityExecutor I { get; private set; }

        // AbilitySO TYPE -> HANDLER
        private readonly Dictionary<Type, IAbilityHandler> handlers = new();

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);

            AutoRegisterHandlers();
        }

        // =====================================================
        // AUTO REGISTRATION
        // =====================================================

        private void AutoRegisterHandlers()
        {
            var handlerTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    !t.IsAbstract &&
                    typeof(IAbilityHandler).IsAssignableFrom(t))
                .ToList();

            foreach (var type in handlerTypes)
            {
                try
                {
                    if (Activator.CreateInstance(type) is not IAbilityHandler handler)
                        continue;

                    var abilityType = handler.AbilityType;
                    if (abilityType == null)
                        continue;

                    if (handlers.ContainsKey(abilityType))
                    {
                        Debug.LogError(
                            $"[AbilityExecutor] Duplicate handler for ability type {abilityType.Name}"
                        );
                        continue;
                    }

                    handlers.Add(abilityType, handler);
                    AbilityHandlerRegistry.Register(handler);

                    Debug.Log(
                        $"[AbilityExecutor] Registered {type.Name} for {abilityType.Name}"
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[AbilityExecutor] Failed to register handler {type.Name}: {ex}"
                    );
                }
            }
        }

        // =====================================================
        // SERVER ENTRY POINT
        // =====================================================

        public void Execute(AbilitySO ability, AbilityContext ctx)
        {
            if (!InstanceFinder.IsServer)
            {
                Debug.LogError("[AbilityExecutor] Execute called not on server");
                return;
            }

            if (ability == null)
            {
                Debug.LogError("[AbilityExecutor] Ability is null");
                return;
            }

            var abilityType = ability.GetType();

            if (!handlers.TryGetValue(abilityType, out var handler))
            {
                Debug.LogError(
                    $"[AbilityExecutor] No handler registered for ability type {abilityType.Name}"
                );
                return;
            }

            handler.ExecuteServer(ability, ctx);
        }
    }
}
