using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DestructibleResourceNode : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Damage Modifiers")]
    [Tooltip("���� �����, � ������� ���� ������ ������.")]
    [SerializeField] private List<DamageType> weaknesses = new List<DamageType>();
    [SerializeField] private float weaknessMultiplier = 2.0f; // ���� * 2

    [Tooltip("���� �����, � ������� ���� ������ ��������.")]
    [SerializeField] private List<DamageType> resistances = new List<DamageType>();
    [SerializeField] private float resistanceMultiplier = 0.1f; // ���� * 0.1

    [Header("Loot Drops")]
    [SerializeField] private Item lootItem;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject destroyEffect;

    [SerializeField] public ParticleSystem destroyFX;
    [SerializeField] public AudioClip destroySfx;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount, DamageType damageType)
    {
        if (currentHealth <= 0) return; // ��� ��������

        // 1. ���������� ����������� �����
        float modifier = 1.0f; // ������� ����
        if (weaknesses.Contains(damageType))
        {
            modifier = weaknessMultiplier;
        }
        else if (resistances.Contains(damageType))
        {
            modifier = resistanceMultiplier;
        }

        // 2. ��������� ����
        float calculatedDamage = damageAmount * modifier;
        currentHealth -= calculatedDamage;

        // 3. �������� ������ ���������
        if (hitEffect != null)
        {
            // TODO: �������� ������ � ����� ��������� (RaycastHit)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // 4. ��������� �� ����������
        if (currentHealth <= 0)
        {
            DestroyNode();
        }
    }

    public void Heal(float amount)
{
}

    private void DestroyNode()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        if (lootItem != null && lootItem.worldPrefab != null)
        {
            int dropCount = Random.Range(minDropCount, maxDropCount + 1);
            for (int i = 0; i < dropCount; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f;
                GameObject lootObject = Instantiate(lootItem.worldPrefab, spawnPos, Quaternion.identity);

                ItemObject itemObj = lootObject.GetComponent<ItemObject>();
                if (itemObj != null)
                {
                    itemObj.itemData = lootItem;
                    itemObj.quantity = 1;
                }
            }
        }

        Destroy(gameObject);
    }
}