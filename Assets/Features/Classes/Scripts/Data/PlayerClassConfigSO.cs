using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Passives.Domain;
using Features.Stats.Application;

namespace Features.Classes.Data
{
    [CreateAssetMenu(menuName = "Orbis/Classes/Class Config")]
    public class PlayerClassConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Preset (Base Stats)")]
        public StatsPresetSO preset;

        [Header("Visual")]
        public RobotVisualPresetSO visualPreset;        

        [Header("Content")]
        public List<PassiveSO> passives;
        public List<AbilitySO> abilities;
    }
}
