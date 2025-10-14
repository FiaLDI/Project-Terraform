using Quests;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Настройки врага")]
    public int health = 3;
    public string enemyType = "Basic";

    [Header("Визуальные эффекты")]
    public Material damageMaterial; 
    private Material originalMaterial;
    private Renderer enemyRenderer;

    void Start()
    {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }

        Debug.Log($"Враг создан: {gameObject.name}, здоровье: {health}");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} получил урон: {damage}. Осталось здоровья: {health}");

        if (enemyRenderer != null && damageMaterial != null)
        {
            enemyRenderer.material = damageMaterial;
            Invoke("ResetMaterial", 0.3f); 
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void ResetMaterial()
    {
        if (enemyRenderer != null && originalMaterial != null)
        {
            enemyRenderer.material = originalMaterial;
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} уничтожен!");

        NotifyQuestSystem();

        Destroy(gameObject);
    }

    void NotifyQuestSystem()
    {
        if (QuestManager.Instance != null)
        {
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questName.Contains("уничтож") || quest.questName.Contains("очист") ||
                    quest.questName.Contains("враг") || quest.questID.Contains("clear"))
                {
                    QuestManager.Instance.UpdateQuestProgress(quest, 1);
                    Debug.Log($"Прогресс квеста '{quest.questName}' обновлен");
                    break;
                }
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"Клик по врагу: {gameObject.name}");
        TakeDamage(3);
    }

    void OnMouseEnter()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
        }
    }

    void OnMouseExit()
    {
        if (enemyRenderer != null && originalMaterial != null)
        {
            enemyRenderer.material = originalMaterial;
        }
    }
}
