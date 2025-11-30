using UnityEngine;


namespace Features.Buffs.Domain
{
    [CreateAssetMenu(menuName = "Game/Buff/Area Buff")]
    public class AreaBuffSO : ScriptableObject
    {
        [Header("Area Settings")]
        public float radius = 10f;
        public LayerMask targetMask;

        [Header("Buff Applied To Targets")]
        public BuffSO buff;
    }
}
