using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class RadiusCircleVisual : MonoBehaviour
    {
        [Header("Ground Snap")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float raycastHeight = 3f;
        [SerializeField] private float raycastDepth = 10f;
        [SerializeField] private float groundOffset = 0.04f;

        [Header("Visual")]
        [SerializeField] private bool snapToGround = true;
        [SerializeField] private bool alignToGroundNormal = true;

        public void SetRadius(float radius, Vector3 origin)
        {
            radius = Mathf.Max(0f, radius);

            transform.localScale = new Vector3(
                radius * 2f,
                radius * 2f,
                1f
            );

            if (snapToGround && TryFindGround(origin, out var groundPoint, out var groundNormal))
            {
                transform.position = groundPoint + groundNormal * groundOffset;

                if (alignToGroundNormal)
                {
                    // Unity Quad после поворота X=90 лежит как раз через local -Z.
                    // ѕоэтому local -forward направл€ем по нормали земли.
                    transform.rotation = Quaternion.FromToRotation(Vector3.back, groundNormal);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }
            else
            {
                transform.position = origin + Vector3.up * groundOffset;
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private bool TryFindGround(Vector3 origin, out Vector3 point, out Vector3 normal)
        {
            Vector3 rayStart = origin + Vector3.up * raycastHeight;
            float rayDistance = raycastHeight + raycastDepth;

            if (Physics.Raycast(
                    rayStart,
                    Vector3.down,
                    out RaycastHit hit,
                    rayDistance,
                    groundMask,
                    QueryTriggerInteraction.Ignore))
            {
                point = hit.point;
                normal = hit.normal;
                return true;
            }

            point = origin;
            normal = Vector3.up;
            return false;
        }
    }
}