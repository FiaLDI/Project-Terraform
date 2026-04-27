using Features.Multiplayer.SceneBinding;
using UnityEngine;

namespace Features.World.Levers
{
    public sealed class LeverView : SceneBoundViewBase
    {
        [SerializeField] private Transform leverHandle;
        [SerializeField] private Vector3 offEuler = new(0f, 0f, 0f);
        [SerializeField] private Vector3 onEuler = new(-45f, 0f, 0f);
        [SerializeField] private float speed = 240f;

        private bool isOn;

        protected override string DefaultBoundType => "lever";

        private void Awake()
        {
            if (leverHandle == null)
                leverHandle = transform;
        }

        public void SetOn(bool value, bool snap = false)
        {
            isOn = value;

            if (snap)
                leverHandle.localRotation = Quaternion.Euler(isOn ? onEuler : offEuler);
        }

        private void Update()
        {
            Quaternion target = Quaternion.Euler(isOn ? onEuler : offEuler);

            leverHandle.localRotation = Quaternion.RotateTowards(
                leverHandle.localRotation,
                target,
                speed * Time.deltaTime
            );
        }
    }
}
