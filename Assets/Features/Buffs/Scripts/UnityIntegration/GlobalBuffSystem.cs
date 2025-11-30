using System.Collections.Generic;
using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Buffs.UnityIntegration
{
    public class GlobalBuffSystem : MonoBehaviour
    {
        public static GlobalBuffSystem I;

        private readonly Dictionary<string, float> values = new();

        private void Awake()
        {
            if (I != null)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
        }

        public void Add(GlobalBuffSO buff)
        {
            if (buff == null) return;

            if (!values.ContainsKey(buff.key))
                values[buff.key] = 0f;

            values[buff.key] += buff.value;
        }

        public void Remove(GlobalBuffSO buff)
        {
            if (buff == null) return;
            if (!values.ContainsKey(buff.key))
                return;

            values[buff.key] -= buff.value;
            if (values[buff.key] <= 0f)
                values.Remove(buff.key);
        }

        public float GetValue(string key)
        {
            return values.TryGetValue(key, out float v) ? v : 0f;
        }
    }
}
