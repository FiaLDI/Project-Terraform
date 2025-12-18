
using UnityEngine;

namespace Features.Weapons.Data
{
    [CreateAssetMenu(menuName = "Configs/Ammo Config")]
    public class AmmoConfig : ScriptableObject
    {
        public string ammoId;
        public Sprite icon;
    }
}
