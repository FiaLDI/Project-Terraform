using System.Collections.Generic;
using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Buffs.UnityIntegration
{
    public sealed class GlobalBuffSystem : MonoBehaviour
    {
        public static GlobalBuffSystem I { get; private set; }

        // Активные инстансы
        private readonly List<GlobalBuffInstance> _active = new();

        // Суммарные значения
        private readonly Dictionary<string, float> _values = new();

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            transform.SetParent(null);
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        // =====================================================
        // ADD
        // =====================================================

        public void Add(GlobalBuffSO buff, IBuffSource source)
        {
            if (buff == null || source == null)
                return;

            if (string.IsNullOrEmpty(buff.key))
                return;

            var inst = new GlobalBuffInstance
            {
                Config = buff,
                Source = source
            };

            _active.Add(inst);

            if (!_values.ContainsKey(buff.key))
                _values[buff.key] = 0f;

            _values[buff.key] += buff.value;
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public void RemoveBySource(IBuffSource source)
        {
            if (source == null)
                return;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var inst = _active[i];
                if (inst.Source == source)
                {
                    RemoveInstance(inst);
                }
            }
        }

        private void RemoveInstance(GlobalBuffInstance inst)
        {
            if (inst == null || inst.Config == null)
                return;

            var buff = inst.Config;

            if (_values.ContainsKey(buff.key))
            {
                _values[buff.key] -= buff.value;
                if (_values[buff.key] <= 0f)
                    _values.Remove(buff.key);
            }

            _active.Remove(inst);
        }

        // =====================================================
        // READ
        // =====================================================

        public float GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0f;

            return _values.TryGetValue(key, out var v) ? v : 0f;
        }
    }
}
