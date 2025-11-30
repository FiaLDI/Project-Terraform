using UnityEngine;
using Features.Combat.Domain;

[RequireComponent(typeof(Collider))]
public class ResourceNode : MonoBehaviour, IMineable, IDamageable
{
    [Header("Node Data")]
    [Tooltip("Конфиг узла ресурса (тип, дроп-таблица, префаб и т.п.).")]
    public ResourceNodeSO nodeData;

    [Header("Health / Mining")]
    [Tooltip("Общее здоровье узла. Используется и для добычи, и для урона.")]
    public float maxHealth = 50f;

    [Tooltip("Текущее здоровье узла.")]
    [SerializeField] private float currentHealth;

    [Header("Effects")]
    [Tooltip("VFX при попадании/добыче.")]
    public GameObject hitEffect;

    [Tooltip("VFX при уничтожении узла.")]
    public GameObject destroyEffect;

    [Tooltip("Отмечает, что узел уже истощён/уничтожен.")]
    public bool isDepleted = false;

    private void Awake()
    {
        if (maxHealth <= 0f)
            maxHealth = 1f;

        currentHealth = maxHealth;
    }

    // ======================================================
    //  IMineable — добыча (бур, кирка, дрель и т.п.)
    // ======================================================
    /// <summary>
    /// amount — сколько "работы" или урона добычи приложено за тик.
    /// tool — используемый инструмент (можно использовать его скорость/тип и т.д.).
    /// </summary>
    public bool Mine(float amount, Tool tool)
    {
        if (isDepleted) return true;

        // Модификатор от инструмента (если нужен)
        if (tool != null)
            amount *= tool.baseHarvestSpeed;

        ApplyDamageInternal(amount, DamageType.Mining);
        return isDepleted;
    }

    /// <summary>
    /// Прогресс добычи 0..1.
    /// </summary>
    public float GetProgress()
    {
        return Mathf.Clamp01(1f - (currentHealth / maxHealth));
    }

    // ======================================================
    //  IDamageable — урон от оружия/взрывов и т.п.
    // ======================================================
    public void TakeDamage(float damageAmount, DamageType damageType)
    {
        if (isDepleted) return;

        ApplyDamageInternal(damageAmount, damageType);
    }

    public void Heal(float amount)
    {
        if (isDepleted) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }

    // ======================================================
    //  Общий метод нанесения урона
    // ======================================================
    private void ApplyDamageInternal(float amount, DamageType damageType)
    {
        if (amount <= 0f) return;

        currentHealth -= amount;

        // Хит-эффект
        if (hitEffect != null)
        {
            var vfx = Instantiate(hitEffect, transform.position, Quaternion.identity);
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(vfx, 3f);
            }
        }

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            OnDepleted();
        }
    }

    // ======================================================
    //  Уничтожение узла + дроп ресурсов
    // ======================================================
    private void OnDepleted()
    {
        if (isDepleted) return;
        isDepleted = true;

        // VFX разрушения
        if (destroyEffect != null)
        {
            var vfx = Instantiate(destroyEffect, transform.position, transform.rotation);
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(vfx, 3f);
            }
        }

        // Дроп ресурсов через твою систему
        if (nodeData != null && nodeData.dropTable != null)
        {
            // Сейчас ResourceDropSystem.Drop() только логирует —
            // позже можно расширить до реального спавна ItemObject.
            ResourceDropSystem.Drop(nodeData.dropTable, transform);
        }

        Destroy(gameObject);
    }
}
