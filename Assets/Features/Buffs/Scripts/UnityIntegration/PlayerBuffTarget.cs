using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Buffs.UnityIntegration
{
    public class PlayerBuffTarget : MonoBehaviour, IBuffTarget
    {
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        public BuffSystem BuffSystem { get; private set; }

        private void Awake()
        {
            BuffSystem = GetComponent<BuffSystem>();

            if (BuffSystem == null)
                Debug.LogError("[PlayerBuffTarget] Missing BuffSystem!", this);
        }
    }
}
