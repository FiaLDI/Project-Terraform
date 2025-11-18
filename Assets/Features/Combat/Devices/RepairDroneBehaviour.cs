using UnityEngine;
using System.Collections;

public class RepairDroneBehaviour : MonoBehaviour
{
    private Transform owner;
    private float lifetime;
    private float healPerSec;
    private float radius;
    private float moveSpeed;

    public void Init(GameObject ownerObj, float life, float heal, float radius, float speed)
    {
        owner = ownerObj.transform;
        lifetime = life;
        healPerSec = heal;
        this.radius = radius;
        moveSpeed = speed;

        StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (!owner) return;

        transform.position = Vector3.Lerp(transform.position, owner.position, Time.deltaTime * moveSpeed);

        Collider[] hits = Physics.OverlapSphere(owner.position, radius);

        foreach (var h in hits)
        {
            var d = h.GetComponent<IDamageable>();
            if (d != null)
                d.Heal(healPerSec * Time.deltaTime);
        }
    }
}
