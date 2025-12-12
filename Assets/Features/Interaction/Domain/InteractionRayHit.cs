using UnityEngine;

namespace Features.Interaction.Domain
{
    public struct InteractionRayHit
    {
        public bool Hit;
        public RaycastHit HitInfo;

        public InteractionRayHit(bool hit, RaycastHit hitInfo)
        {
            Hit = hit;
            HitInfo = hitInfo;
        }
    }
}
