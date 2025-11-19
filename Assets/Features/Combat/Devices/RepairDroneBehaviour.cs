using UnityEngine;

public class RepairDroneBehaviour : MonoBehaviour
{
    private GameObject owner;
    private float lifetime;
    private float speed;

    public void Init(GameObject owner, float lifetime, float speed)
    {
        this.owner = owner;
        this.lifetime = lifetime;
        this.speed = speed;
    }

    private void Update()
    {
        if (!owner) return;

        Vector3 target = owner.transform.position + new Vector3(2f, 3f, 0);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
    }
}
