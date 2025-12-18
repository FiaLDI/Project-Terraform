using UnityEngine;

namespace Features.Passives.Domain
{
    public abstract class PassiveSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        
        /// <summary>Активировать пассивку для конкретного владельца.</summary>
        public abstract void Apply(GameObject owner);
        
        /// <summary>Отключить пассивку для конкретного владельца.</summary>
        public abstract void Remove(GameObject owner);
    }
}
