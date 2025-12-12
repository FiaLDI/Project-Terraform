using UnityEngine;
using Features.Resources.Domain;
using Features.Items.Data;

namespace Features.Resources.Data 
{
    [CreateAssetMenu(menuName = "Orbis/Resources/Resource Bundle")]
    public class ResourceSO : ScriptableObject
    {
        [Header("Base Info")]
        public string resourceName;
        public Sprite icon;
        public Color color = Color.white;

        [Header("Item Settings")]
        public Item item; //Что выпадет игроку

        [Header("Node Settings")]
        public GameObject nodePrefab;
        public float maxHealth = 50f;
        public GameObject hitEffect;
        public GameObject destroyEffect;

        [Header("Spawn Settings")]
        public float noiseThreshold = 0.5f;
        public float minDistance = 10f;

        [Header("Drop Table")]
        public DropEntry[] drops;
    }
}
