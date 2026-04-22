using UnityEngine;
using FishNet.Object;
using Features.Effects.Application;

namespace Features.Combat.Devices
{
    public sealed class RepairDroneBehaviour : NetworkBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(2f, 3f, 0f);

        private Transform followTarget;

        private void Start()
        {
            if (!IsServerInitialized)
                return;

            var ctx = GetComponent<SpawnedObjectContext>();
            if (ctx != null && ctx.Source != null)
            {
                followTarget = ctx.Source.transform;
            }
        }

        private void Update()
        {
            if (!IsServerInitialized)
                return;

            if (followTarget == null)
                return;

            Vector3 target = followTarget.position + offset;

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
        }
    }
}
