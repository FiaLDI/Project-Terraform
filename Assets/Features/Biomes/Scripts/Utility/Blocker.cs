using UnityEngine;

[System.Serializable]
public struct Blocker
{
    public Vector3 position;
    public float radius;

    public Blocker(Vector3 position, float radius)
    {
        this.position = position;
        this.radius = radius;
    }
}
