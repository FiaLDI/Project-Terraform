using UnityEngine;

public class WorldGenerator
{
    private WorldConfig world;

    public WorldGenerator(WorldConfig world)
    {
        this.world = world;
    }

    public BiomeConfig GetBiomeForChunk(Vector2Int coord)
    {
        if (world.biomes == null || world.biomes.Length == 0)
            return null;

        int index = Mathf.Abs(coord.x + coord.y) % world.biomes.Length;
        return world.biomes[index].config;
    }
}
