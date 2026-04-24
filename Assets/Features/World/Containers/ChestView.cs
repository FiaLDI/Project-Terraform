using Features.Multiplayer.SceneBinding;
using UnityEngine;

namespace Features.World.Containers
{
    public sealed class ChestView : SceneBoundViewBase
    {
        [SerializeField] private Transform lid;
        [SerializeField] private Vector3 closedEuler;
        [SerializeField] private Vector3 openEuler = new(-70f, 0f, 0f);
        [SerializeField] private float speed = 180f;

        private bool isOpen;

        protected override string DefaultBoundType => "chest";

        private void Awake()
        {
            if (lid == null)
                lid = transform;
        }

        public void SetOpen(bool value, bool snap = false)
        {
            isOpen = value;

            if (snap)
                lid.localRotation = Quaternion.Euler(isOpen ? openEuler : closedEuler);
        }

        private void Update()
        {
            Quaternion target = Quaternion.Euler(isOpen ? openEuler : closedEuler);

            lid.localRotation = Quaternion.RotateTowards(
                lid.localRotation,
                target,
                speed * Time.deltaTime
            );
        }
    }
}
