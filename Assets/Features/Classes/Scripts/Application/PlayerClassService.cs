using System.Collections.Generic;
using Features.Classes.Data;
using Features.Classes.Domain;

namespace Features.Classes.Application
{
    public class PlayerClassService : IPlayerClassService
    {
        private readonly Dictionary<string, PlayerClassConfigSO> _classes =
            new Dictionary<string, PlayerClassConfigSO>();

        public PlayerClassConfigSO Current { get; private set; }

        public IEnumerable<PlayerClassConfigSO> AllClasses => _classes.Values;

        public PlayerClassService(IEnumerable<PlayerClassConfigSO> classConfigs, string defaultClassId = null)
        {
            foreach (var cfg in classConfigs)
            {
                if (cfg == null || string.IsNullOrWhiteSpace(cfg.id))
                    continue;

                _classes[cfg.id] = cfg;
            }

            if (!string.IsNullOrWhiteSpace(defaultClassId))
                SelectClass(defaultClassId);
        }

        public PlayerClassConfigSO FindById(string id)
        {
            if (_classes.TryGetValue(id, out var cfg))
                return cfg;

            return null;
        }

        public void SelectClass(string id)
        {
            var cfg = FindById(id);
            if (cfg == null)
                throw new KeyNotFoundException($"Class '{id}' not found");

            Current = cfg;
        }

        public void SelectClass(PlayerClassConfigSO config)
        {
            if (config == null)
                throw new System.ArgumentNullException(nameof(config));

            Current = config;
        }
    }
}
