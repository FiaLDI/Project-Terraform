using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResourceNode : MonoBehaviour
{
    [Tooltip("Ссылка на ScriptableObject, который описывает этот узел.")]
    public ResourceNodeSO nodeData;

    [Tooltip("Префаб VFX, который будет проигрываться при добыче (опционально).")]
    public GameObject harvestVFX;

    [Tooltip("Если true — узел уже добыт/не доступен.")]
    public bool isDepleted = false;

    [Header("Harvesting")]
    public int hitPoints = 1;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = false;
    }

    public void ApplyHarvest()
    {
        if (isDepleted) return;

        hitPoints--;
        if (hitPoints <= 0)
        {
            OnHarvested();
        }
        else
        {

        }
    }

    private void OnHarvested()
    {
        isDepleted = true;


        if (nodeData != null && nodeData.dropTable != null)
        {
            ResourceDropSystem.Drop(nodeData.dropTable, transform);
        }
        else
        {
            Debug.LogWarning($"ResourceNode ({name}) has no drop table assigned in nodeData.");
        }

        if (harvestVFX != null)
        {
            var vfx = Instantiate(harvestVFX, transform.position, Quaternion.identity);
            var ps = vfx.GetComponentInChildren<ParticleSystem>();
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

        Destroy(gameObject);
    }
}
