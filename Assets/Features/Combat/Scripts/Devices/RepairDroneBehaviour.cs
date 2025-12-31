using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Player.UnityIntegration;

namespace Features.Combat.Devices
{
    public sealed class RepairDroneBehaviour : MonoBehaviour
    {
        private Transform followTarget;
        private float speed;

        public void Init(GameObject owner, float lifetime, float speed)
        {
            this.followTarget = owner.transform;
            this.speed = speed;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!followTarget)
                return;

            Vector3 target = followTarget.position + new Vector3(2f, 3f, 0);
            transform.position = Vector3.Lerp(
                transform.position,
                target,
                Time.deltaTime * speed
            );
        }
    }

}
