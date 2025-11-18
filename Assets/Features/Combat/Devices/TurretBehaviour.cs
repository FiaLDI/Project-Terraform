using UnityEngine;
using System.Collections;

public class TurretBehaviour : MonoBehaviour, IDamageable
{
    public float damagePerSecond;
    public float range;
    public float lifetime;
    public int maxHp;

    private int currentHp;
    private Transform owner;
    private Transform target;

    public void Init(GameObject owner, int hp, float dps, float range, float lifetime)
    {
        this.owner = owner.transform;
        maxHp = hp;
        currentHp = hp;

        damagePerSecond = dps;
        this.range = range;
        this.lifetime = lifetime;

        StartCoroutine(LifeTimer());
    }


    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void Update()
    {
        AcquireTarget();
        AttackTarget();
    }

    void AcquireTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        float closest = Mathf.Infinity;
        target = null;

        foreach (var h in hits)
        {
            if (h.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(h.transform.position, transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    target = h.transform;
                }
            }
        }
    }

    void AttackTarget()
    {
        if (target == null) return;

        IDamageable dmg = target.GetComponent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(damagePerSecond * Time.deltaTime, DamageType.Generic);
    }

    public void TakeDamage(float amount, DamageType type)
    {
        currentHp -= Mathf.RoundToInt(amount);
        if (currentHp <= 0) Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + Mathf.RoundToInt(amount));
    }
}
