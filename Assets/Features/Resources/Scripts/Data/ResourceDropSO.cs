using UnityEngine;

[CreateAssetMenu(fileName = "ResourceDrop", menuName = "Resources/Resource Drop")]
public class ResourceDropSO : ScriptableObject
{
    [System.Serializable]
    public class DropItem
    {
        public ResourceSO resource; 
        public int minAmount = 1; 
        public int maxAmount = 3;   
        [Range(0f, 1f)]
        public float chance = 1f;
    }

    public DropItem[] drops;
}
