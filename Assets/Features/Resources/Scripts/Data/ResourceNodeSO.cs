using UnityEngine;

[CreateAssetMenu(fileName = "ResourceNode", menuName = "Resources/Resource Node")]
public class ResourceNodeSO : ScriptableObject
{
    [Header("Identification")]
    public string nodeName;        
    public ResourceSO resource;     

    [Header("Prefab")]
    public GameObject prefab;      

    [Header("Generation")]
    [Range(0f, 1f)]
    public float noiseThreshold = 0.5f; 
    public float minDistance = 10f;    

    [Header("Drops")]
    public ResourceDropSO dropTable;   
}
