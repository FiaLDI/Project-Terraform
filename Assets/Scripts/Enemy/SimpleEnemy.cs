using Quests;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("��������� �����")]
    public int health = 3;
    public string enemyType = "Basic";

    [Header("���������� �������")]
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

        Debug.Log($"���� ������: {gameObject.name}, ��������: {health}");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} ������� ����: {damage}. �������� ��������: {health}");

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
        Debug.Log($"{gameObject.name} ���������!");

        NotifyQuestSystem();

        Destroy(gameObject);
    }

    void NotifyQuestSystem()
    {
        if (QuestManager.Instance != null)
        {
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questName.Contains("�������") || quest.questName.Contains("�����") ||
                    quest.questName.Contains("����") || quest.questID.Contains("clear"))
                {
                    QuestManager.Instance.UpdateQuestProgress(quest);

                    Debug.Log($"�������� ������ '{quest.questName}' ��������");
                    break;
                }
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"���� �� �����: {gameObject.name}");
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
