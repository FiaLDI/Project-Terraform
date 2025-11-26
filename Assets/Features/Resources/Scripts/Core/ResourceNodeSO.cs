using UnityEngine;

public class ResourceNodeSO : ScriptableObject
{
    public string nodeName;        
    public ResourceSO resource;     
    public GameObject prefab;      
    [Range(0f, 1f)] public float noiseThreshold = 0.5f; 
    public float minDistance = 10f;    
    public ResourceDropSO dropTable;   
}
