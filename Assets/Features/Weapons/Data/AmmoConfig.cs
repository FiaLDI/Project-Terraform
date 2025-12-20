
using UnityEngine;

namespace Features.Weapons.Data
{
    [CreateAssetMenu(menuName = "Items/Configs/Ammo Config")]
    public class AmmoConfig : ScriptableObject
    {
        public string ammoId;
        public Sprite icon;
    }
}
