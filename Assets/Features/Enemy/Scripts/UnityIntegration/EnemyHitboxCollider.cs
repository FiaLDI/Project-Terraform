using UnityEngine;
using Features.Enemy.Domain;
using Features.Combat.Domain;

namespace Features.Enemy.UnityIntegration
{
    public class EnemyHitboxCollider : MonoBehaviour
    {
        public HitboxType hitboxType;

        private EnemyActor root;

        private void Awake()
        {
            root = GetComponentInParent<EnemyActor>();
        }

        public void ApplyHit(HitInfo hit)
        {
            if (root != null)
                root.OnHitboxHit(hit, hitboxType);
        }
    }
}
