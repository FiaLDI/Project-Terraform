using UnityEngine;
using System.Collections;

public class RepairDroneBehaviour : MonoBehaviour
{
    private Transform owner;
    private float lifetime;
    private float healPerSec;
    private float radius;
    private float moveSpeed;
    private EnergyConsumer energyConsumer;

    // Смещение дрона относительно игрока
    private Vector3 offset = new Vector3(-1f, 3f, 0f);

    private void Awake()
    {
        energyConsumer = GetComponent<EnergyConsumer>();
    }

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

        // локальное смещение вокруг игрока (левее = -right, выше = up)
        Vector3 targetPos =
            owner.position
            + owner.right * offset.x * -1f
            + owner.up * offset.y
            + owner.forward * offset.z;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );

        // лечение
        Collider[] hits = Physics.OverlapSphere(owner.position, radius);

        foreach (var h in hits)
        {
            if (h.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.Heal(healPerSec * Time.deltaTime);
            }
        }
    }
}
