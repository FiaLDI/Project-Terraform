using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Apply Buff To Owner")]
    public class PassiveEffect_ApplyBuffToOwnerSO : PassiveEffectSO
    {
        public BuffSO buff;

        private BuffInstance _instance;

        public override void Apply(GameObject owner)
        {
            if (buff == null) return;

            var system = owner.GetComponent<BuffSystem>();
            if (system == null) return;

            _instance = system.Add(buff);
        }

        public override void Remove(GameObject owner)
        {
            if (_instance == null) return;

            var system = owner.GetComponent<BuffSystem>();
            if (system == null) return;

            system.Remove(_instance);
            _instance = null;
        }
    }
}
