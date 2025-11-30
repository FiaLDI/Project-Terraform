using UnityEngine;

public class FollowPlayerStore : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 offset;

    void Update()
    {
        if (!PlayerPositionStore.Player) return;

        Vector3 targetPos = PlayerPositionStore.Player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
    }
}
