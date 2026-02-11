using UnityEngine;
using FishNet;
using Features.Abilities.Domain;
using Features.Effects.Application;
using Features.Effects.Domain;

namespace Features.Abilities.UnityIntegration
{
    public sealed class AbilityExecutor : MonoBehaviour
    {
        public static AbilityExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Execute(AbilitySO ability, AbilityContext ctx)
        {
            if (!InstanceFinder.IsServer)
                return;

            if (ability == null || ability.effects == null)
                return;

            var baseContext = new EffectContext(
                source: ctx.Owner,
                targets: null,
                origin: ctx.TargetPoint,
                direction: ctx.Direction
            );

            foreach (var def in ability.effects)
            {
                EffectExecutor.Instance.Execute(def, baseContext);
            }
        }
    }
}
