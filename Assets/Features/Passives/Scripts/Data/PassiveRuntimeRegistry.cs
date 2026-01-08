using System.Collections.Generic;
using UnityEngine;

namespace Features.Passives.Domain
{
    /// <summary>
    /// Хранит runtime-состояние пассивных эффектов.
    /// Ключ: (owner, PassiveEffectSO)
    /// </summary>
    public static class PassiveRuntimeRegistry
    {
        // owner -> (effect -> runtime)
        private static readonly Dictionary<GameObject, Dictionary<PassiveEffectSO, PassiveRuntime>> data
            = new();

        // =====================================================
        // STORE
        // =====================================================

        public static void Store(
            GameObject owner,
            PassiveEffectSO effect,
            PassiveRuntime runtime)
        {
            if (owner == null || effect == null || runtime == null)
                return;

            if (!data.TryGetValue(owner, out var map))
            {
                map = new Dictionary<PassiveEffectSO, PassiveRuntime>();
                data[owner] = map;
            }

            // защита от дублей
            if (map.ContainsKey(effect))
            {
                Debug.LogWarning(
                    $"[PassiveRuntimeRegistry] Runtime already exists for effect '{effect.name}' on '{owner.name}'"
                );
                return;
            }

            map[effect] = runtime;
        }

        // =====================================================
        // TAKE (REMOVE + RETURN)
        // =====================================================

        public static PassiveRuntime Take(
            GameObject owner,
            PassiveEffectSO effect)
        {
            if (owner == null || effect == null)
                return null;

            if (!data.TryGetValue(owner, out var map))
                return null;

            if (!map.TryGetValue(effect, out var runtime))
                return null;

            map.Remove(effect);

            if (map.Count == 0)
                data.Remove(owner);

            return runtime;
        }

        // =====================================================
        // QUERY
        // =====================================================

        public static bool Has(
            GameObject owner,
            PassiveEffectSO effect)
        {
            return owner != null
                && effect != null
                && data.TryGetValue(owner, out var map)
                && map.ContainsKey(effect);
        }

        // =====================================================
        // CLEAR OWNER (например, при despawn)
        // =====================================================

        public static void ClearOwner(GameObject owner)
        {
            if (owner == null)
                return;

            data.Remove(owner);
        }

        // =====================================================
        // FULL CLEAR (debug / scene reload)
        // =====================================================

        public static void ClearAll()
        {
            data.Clear();
        }
    }
}
