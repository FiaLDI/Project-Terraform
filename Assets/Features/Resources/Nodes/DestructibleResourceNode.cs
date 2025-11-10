using UnityEngine;
using System.Collections.Generic;

// Этот компонент вешается на префаб руды, ящика, дерева и т.д.
[RequireComponent(typeof(Collider))]
public class DestructibleResourceNode : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Damage Modifiers")]
    [Tooltip("Типы урона, к которым этот объект уязвим.")]
    [SerializeField] private List<DamageType> weaknesses = new List<DamageType>();
    [SerializeField] private float weaknessMultiplier = 2.0f; // Урон * 2

    [Tooltip("Типы урона, к которым этот объект устойчив.")]
    [SerializeField] private List<DamageType> resistances = new List<DamageType>();
    [SerializeField] private float resistanceMultiplier = 0.1f; // Урон * 0.1

    [Header("Loot Drops")]
    [SerializeField] private Item lootItem; // ScriptableObject предмета, который выпадет
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect; // Эффект при ударе (искры)
    [SerializeField] private GameObject destroyEffect; // Эффект при разрушении (взрыв, пыль)

    [SerializeField] public ParticleSystem destroyFX;
    [SerializeField] public AudioClip destroySfx;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // Реализация интерфейса IDamageable
    public void TakeDamage(float damageAmount, DamageType damageType)
    {
        if (currentHealth <= 0) return; // Уже разрушен

        // 1. Рассчитать модификатор урона
        float modifier = 1.0f; // Обычный урон
        if (weaknesses.Contains(damageType))
        {
            modifier = weaknessMultiplier;
        }
        else if (resistances.Contains(damageType))
        {
            modifier = resistanceMultiplier;
        }

        // 2. Применить урон
        float calculatedDamage = damageAmount * modifier;
        currentHealth -= calculatedDamage;

        // 3. Показать эффект попадания
        if (hitEffect != null)
        {
            // TODO: Спавнить эффект в точке попадания (RaycastHit)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // 4. Проверить на разрушение
        if (currentHealth <= 0)
        {
            DestroyNode();
        }
    }

    private void DestroyNode()
    {
        // 1. Показать эффект разрушения
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        // 2. Выбросить лут
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
                    itemObj.quantity = 1; // Выбрасываем по 1, но в кол-ве dropCount
                }
            }
        }

        // 3. Уничтожить объект руды
        Destroy(gameObject);
    }
}