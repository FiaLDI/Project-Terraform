using UnityEngine;
using Features.Buffs.Application;
using Features.Buffs.Domain;

namespace Features.Buffs.UnityIntegration
{
    /// <summary>
    /// Единственный мост между Unity и чистым GlobalBuffRegistry.
    /// </summary>
    public class GlobalBuffRegistryAdapter : MonoBehaviour
    {
        public static IGlobalBuffRegistry I { get; private set; }

        private void Awake()
        {
            if (I != null)
            {
                Destroy(gameObject);
                return;
            }

            I = new GlobalBuffRegistry();
            DontDestroyOnLoad(gameObject);

            Debug.Log("[GlobalBuffRegistry] Initialized");
        }

        public void Apply(GlobalBuffSO buff)
        {
            if (buff == null) return;
            I.Add(buff.key, buff.value);
        }

        public void Remove(GlobalBuffSO buff)
        {
            if (buff == null) return;
            I.Remove(buff.key, buff.value);
        }
    }
}
