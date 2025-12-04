using System.Collections.Generic;
using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Buffs.UnityIntegration
{
    public class GlobalBuffSystem : MonoBehaviour
    {
        public static GlobalBuffSystem I { get; private set; }

        private readonly Dictionary<string, float> _values = new();

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
        }

        public void Add(GlobalBuffSO buff)
        {
            if (buff == null) return;
            if (string.IsNullOrEmpty(buff.key)) return;

            if (!_values.ContainsKey(buff.key))
                _values[buff.key] = 0f;

            _values[buff.key] += buff.value;
        }

        public void Remove(GlobalBuffSO buff)
        {
            if (buff == null) return;
            if (string.IsNullOrEmpty(buff.key)) return;
            if (!_values.ContainsKey(buff.key)) return;

            _values[buff.key] -= buff.value;
            if (_values[buff.key] <= 0f)
                _values.Remove(buff.key);
        }

        public float GetValue(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0f;
            return _values.TryGetValue(key, out float v) ? v : 0f;
        }
    }
}
