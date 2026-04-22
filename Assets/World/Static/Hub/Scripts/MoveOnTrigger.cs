using UnityEngine;
using FishNet.Object;

public class MoveOnTrigger : NetworkBehaviour
{
    [Header("Door")]
    public Rigidbody objectToMove;
    public Vector3 localMoveDirection = new Vector3(0, 0, 1);
    public float speed = 2f;
    public float moveDistance = 3f;

    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private bool _shouldOpen;

    private void Start()
    {
        if (objectToMove == null)
        {
            return;
        }

        _closedPosition = objectToMove.position;

        Vector3 worldDirection =
            objectToMove.transform.TransformDirection(localMoveDirection.normalized);

        _openPosition = _closedPosition + worldDirection * moveDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!IsServerInitialized)
        {
            return;
        }

        _shouldOpen = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!IsServerInitialized)
        {
            return;
        }

        _shouldOpen = false;
    }

    private void FixedUpdate()
    {
        if (!IsServerInitialized || objectToMove == null)
            return;

        Vector3 target = _shouldOpen ? _openPosition : _closedPosition;
        Vector3 current = objectToMove.position;
        Vector3 next = Vector3.MoveTowards(current, target, speed * Time.fixedDeltaTime);

        if ((current - target).sqrMagnitude > 0.0001f)
        {
            objectToMove.MovePosition(next);
        }
    }
}
