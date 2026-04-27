using Features.Multiplayer.SceneBinding;
using UnityEngine;

namespace Features.World.Doors
{
    public sealed class DoorView : SceneBoundViewBase
    {
        [Header("Door")]
        [SerializeField] private Transform objectToMove;
        [SerializeField] private Vector3 localMoveDirection = new(0f, 0f, 1f);
        [SerializeField] private float moveDistance = 3f;
        [SerializeField] private float speed = 2f;

        [Header("Activation")]
        [SerializeField] private DoorActivationMode activationMode = DoorActivationMode.TriggerAndInteract;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private bool isOpen;
        private Rigidbody objectBody;
        private int debugMoveFramesRemaining;

        public DoorActivationMode ActivationMode => activationMode;

        protected override string DefaultBoundType => "door";

        private void Awake()
        {
            if (objectToMove == null)
                objectToMove = transform;

            objectBody = objectToMove.GetComponent<Rigidbody>();
            closedPosition = objectToMove.position;

            Vector3 worldDirection = objectToMove.TransformDirection(localMoveDirection.normalized);
            openPosition = closedPosition + worldDirection * moveDistance;

            Debug.Log(
                $"[DoorView] Awake view={name} object={objectToMove.name} closed={closedPosition} open={openPosition} hasBody={objectBody != null}",
                this
            );
        }

        public void SetOpen(bool value, bool snap = false)
        {
            isOpen = value;
            debugMoveFramesRemaining = snap ? 0 : 8;

            Debug.Log(
                $"[DoorView] SetOpen view={name} isOpen={isOpen} snap={snap} current={objectToMove.position} target={(isOpen ? openPosition : closedPosition)}",
                this
            );

            if (snap)
            {
                if (objectBody != null)
                    objectBody.position = isOpen ? openPosition : closedPosition;
                else
                    objectToMove.position = isOpen ? openPosition : closedPosition;
            }
        }

        private void Update()
        {
            Vector3 target = isOpen ? openPosition : closedPosition;
            Vector3 current = objectToMove.position;
            Vector3 next = Vector3.MoveTowards(
                current,
                target,
                speed * Time.deltaTime
            );

            if (objectBody == null)
                objectToMove.position = next;

            if (debugMoveFramesRemaining > 0)
            {
                debugMoveFramesRemaining--;
                Debug.Log(
                    $"[DoorView] Update view={name} current={current} next={next} target={target} remaining={(target - next).magnitude}",
                    this
                );
            }
        }

        private void FixedUpdate()
        {
            if (objectBody == null)
                return;

            Vector3 target = isOpen ? openPosition : closedPosition;
            Vector3 current = objectBody.position;
            Vector3 next = Vector3.MoveTowards(
                current,
                target,
                speed * Time.fixedDeltaTime
            );

            objectBody.MovePosition(next);

            if (debugMoveFramesRemaining > 0)
            {
                debugMoveFramesRemaining--;
                Debug.Log(
                    $"[DoorView] FixedUpdate view={name} current={current} next={next} target={target} remaining={(target - next).magnitude}",
                    this
                );
            }
        }
    }
}
