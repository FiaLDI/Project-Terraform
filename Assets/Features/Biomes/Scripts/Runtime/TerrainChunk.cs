using UnityEngine;

public class TerrainChunk : MonoBehaviour {
    public Vector2Int coord;
    public bool isLoaded;
    public void Load() { gameObject.SetActive(true); isLoaded = true; }
    public void Unload() { gameObject.SetActive(false); isLoaded = false; }
}
