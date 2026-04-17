using System.Collections.Generic;
using UnityEngine;

namespace Features.Classes.Data
{
    [CreateAssetMenu(menuName = "Game/Classes/Class Library")]
    public class PlayerClassLibrarySO : ScriptableObject
    {
        public List<PlayerClassConfigSO> classes;

        public PlayerClassConfigSO FindById(string id)
        {
            foreach (var cfg in classes)
            {
                if (cfg != null && cfg.id == id)
                    return cfg;
            }

            return null;
        }
    }
}
