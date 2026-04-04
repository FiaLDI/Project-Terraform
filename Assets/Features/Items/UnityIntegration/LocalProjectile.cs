using UnityEngine;

public class LocalProjectile : MonoBehaviour
{
    private Vector3 velocity;

    public void Init(Vector3 dir, float speed)
    {
        velocity = dir * speed;
        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
