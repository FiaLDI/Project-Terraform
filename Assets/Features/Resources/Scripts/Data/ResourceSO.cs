using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Resources/Resource")]
public class ResourceSO : ScriptableObject
{
    public string resourceName; 
    public Sprite icon;         
    public Color color;         
}
