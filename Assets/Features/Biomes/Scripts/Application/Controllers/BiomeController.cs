using UnityEngine;
using Features.Biomes.Domain;

public class BiomeController : MonoBehaviour
{
    public WorldConfig world;
    public Transform player;
    public int loadDistance = 5;
    public int unloadDistance = 7;

    private ChunkManager chunkManager;

    private void Start()
    {
        if (world == null)
        {
            Debug.LogError("BiomeController: WorldConfig is not assigned!");
            return;
        }

        chunkManager = new ChunkManager(world);
    }

    private void Update()
    {
        if (player == null || chunkManager == null) return;

        chunkManager.UpdateChunks(player.position, loadDistance, unloadDistance);
    }
}
