using UnityEngine;

namespace Features.Combat.Devices
{
    public class ChargeDeviceBehaviour : MonoBehaviour
    {
        private float _duration;
        private Transform _owner;
        private float _deathTime;

        public void Init(Transform owner, float duration)
        {
            _owner = owner;
            _duration = duration;
            _deathTime = Time.time + duration;
        }

        private void Update()
        {
            if (_owner != null)
                transform.position = _owner.position;

            if (Time.time >= _deathTime)
                Destroy(gameObject);
        }
    }
}
