using UnityEngine;
using FishNet;

namespace Features.Combat.Devices
{
    [RequireComponent(typeof(SphereCollider))]
    public class ShieldGridBehaviour : MonoBehaviour
    {
        [Header("Shield Settings")]
        [SerializeField] private float pushForce = 15f;
        [SerializeField] private LayerMask Enemy;

        private SphereCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true; // работаем как зона
        }

        private void OnTriggerStay(Collider other)
        {
            // Авторитет только сервер
            if (!InstanceFinder.IsServer)
                return;

            // Только враги
            if ((Enemy.value & (1 << other.gameObject.layer)) == 0)
                return;

            if (!other.attachedRigidbody)
                return;

            Vector3 dir = (other.transform.position - transform.position).normalized;

            other.attachedRigidbody.AddForce(
                dir * pushForce,
                ForceMode.Force
            );
        }
    }
}