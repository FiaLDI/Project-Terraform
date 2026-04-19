using UnityEngine;

public class MoveOnTrigger : MonoBehaviour
{
    public Transform objectToMove;   // Объект, который будем двигать
    public Vector3 moveDirection = new Vector3(0, 0, 1); // Направление движения
    public float speed = 5f;

    private bool shouldMove = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        shouldMove = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        shouldMove = false;
    }

    private void Update()
    {
        if (shouldMove && objectToMove != null)
        {
            objectToMove.Translate(moveDirection * speed * Time.deltaTime);
        }
    }
}