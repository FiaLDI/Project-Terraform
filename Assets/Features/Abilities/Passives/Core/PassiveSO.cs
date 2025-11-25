using UnityEngine;

public abstract class PassiveSO : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    public abstract void Apply(GameObject owner);
    public abstract void Remove(GameObject owner);
}
