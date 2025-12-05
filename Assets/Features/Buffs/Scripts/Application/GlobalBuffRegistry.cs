using System.Collections.Generic;
using Features.Buffs.Domain;

namespace Features.Buffs.Application
{
    /// <summary>
    /// Полностью чистый, тестируемый, без Unity зависимостей.
    /// </summary>
    public class GlobalBuffRegistry : IGlobalBuffRegistry
    {
        private readonly Dictionary<string, float> values = new();

        public float GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return 0f;

            return values.TryGetValue(key, out var v) ? v : 0f;
        }

        public void Add(string key, float value)
        {
            if (!values.ContainsKey(key))
                values[key] = 0f;

            values[key] += value;
        }

        public void Remove(string key, float value)
        {
            if (!values.ContainsKey(key))
                return;

            values[key] -= value;

            if (values[key] <= 0)
                values.Remove(key);
        }
    }
}
