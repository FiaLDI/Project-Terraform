using Features.Multiplayer.SceneBinding;
using UnityEngine;

namespace Features.World.Elevators
{
    public sealed class ElevatorView : SceneBoundViewBase
    {
        [SerializeField] private Transform platform;
        [SerializeField] private Transform[] floors;
        [SerializeField] private float speed = 2f;

        private int targetFloor;

        public int FloorCount => floors != null ? floors.Length : 0;

        protected override string DefaultBoundType => "elevator";

        private void Awake()
        {
            if (platform == null)
                platform = transform;
        }

        public void SetFloor(int floorIndex, bool snap = false)
        {
            if (floors == null || floors.Length == 0)
                return;

            targetFloor = Mathf.Clamp(floorIndex, 0, floors.Length - 1);

            if (snap)
                platform.position = floors[targetFloor].position;
        }

        private void Update()
        {
            if (floors == null || floors.Length == 0)
                return;

            Vector3 target = floors[targetFloor].position;

            platform.position = Vector3.MoveTowards(
                platform.position,
                target,
                speed * Time.deltaTime
            );
        }
    }
}
