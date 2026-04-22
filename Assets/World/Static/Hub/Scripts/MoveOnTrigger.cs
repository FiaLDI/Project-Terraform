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
            Debug.LogError("[MoveOnTrigger] objectToMove is NULL");
            return;
        }

        _closedPosition = objectToMove.position;

        Vector3 worldDirection =
            objectToMove.transform.TransformDirection(localMoveDirection.normalized);

        _openPosition = _closedPosition + worldDirection * moveDistance;

        Debug.Log($"[MoveOnTrigger] Object={objectToMove.name}");
        Debug.Log($"[MoveOnTrigger] Closed={_closedPosition}");
        Debug.Log($"[MoveOnTrigger] Open={_openPosition}");
        Debug.Log($"[MoveOnTrigger] WorldDirection={worldDirection}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Trigger Enter] {other.name}, tag={other.tag}");

        if (!other.CompareTag("Player"))
            return;

        if (!IsServerInitialized)
        {
            Debug.Log("[Trigger Enter] ignored, not server");
            return;
        }

        _shouldOpen = true;
        Debug.Log("[Server] Door OPEN");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Trigger Exit] {other.name}, tag={other.tag}");

        if (!other.CompareTag("Player"))
            return;

        if (!IsServerInitialized)
        {
            Debug.Log("[Trigger Exit] ignored, not server");
            return;
        }

        _shouldOpen = false;
        Debug.Log("[Server] Door CLOSE");
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
            Debug.Log($"[Server Move] current={current} target={target} next={next}");
        }
    }
}
