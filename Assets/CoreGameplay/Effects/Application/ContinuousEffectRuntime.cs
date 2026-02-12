using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class ContinuousEffectRuntime : MonoBehaviour
    {
        public static ContinuousEffectRuntime Instance;

        private readonly Dictionary<object, Coroutine> running =
            new Dictionary<object, Coroutine>();

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartContinuous(
            object key,
            float interval,
            EffectDefinition[] defs,
            EffectContext baseContext)
        {
            if (!InstanceFinder.IsServer)
                return;
            
            Debug.Log("[Continuous] START for " + key);

            StopContinuous(key);

            running[key] = StartCoroutine(
                Run(interval, defs, baseContext)
            );
        }

        public void StopContinuous(object key)
        {
            if (!running.TryGetValue(key, out var co))
                return;

            StopCoroutine(co);
            running.Remove(key);
        }

        private IEnumerator Run(
            float interval,
            EffectDefinition[] defs,
            EffectContext baseContext)
        {
            var wait = new WaitForSeconds(interval);

            while (true)
            {
                foreach (var def in defs)
                    EffectExecutor.Instance.Execute(def, baseContext);

                yield return wait;
            }
        }
    }
}
