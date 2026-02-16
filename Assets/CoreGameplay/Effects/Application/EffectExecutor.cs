using UnityEngine;
using FishNet;
using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class EffectExecutor : MonoBehaviour
    {
        public static EffectExecutor Instance { get; private set; }

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

        public void Execute(EffectDefinition def, EffectContext baseContext)
        {
            if (!InstanceFinder.IsServer)
                return;

            var targets = TargetResolver.Resolve(def, baseContext);

            var finalContext = new EffectContext(
                source: baseContext.Source,
                targets: targets,
                origin: baseContext.Origin,
                direction: baseContext.Direction
            );

            var effect = EffectFactory.Create(def);
            effect?.Apply(finalContext);
        }

    }
}
