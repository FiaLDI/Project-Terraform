using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using FishNet.Object;


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

            var netObj = owner.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsServer)
            {
                Debug.Log($"[PassiveEffect_ApplyBuffToOwnerSO] On client - skipping buff add (will sync via SyncList)");
                return;
            }

            _instance = system.Add(buff);
            Debug.Log($"[PassiveEffect_ApplyBuffToOwnerSO] Applied buff '{buff.buffId}' to {owner.name}");
        }


        public override void Remove(GameObject owner)
        {
            if (_instance == null) return;

            var system = owner.GetComponent<BuffSystem>();
            if (system == null) return;

            system.Remove(_instance);
            _instance = null;
            Debug.Log($"[PassiveEffect_ApplyBuffToOwnerSO] Removed buff");
        }
    }
}
