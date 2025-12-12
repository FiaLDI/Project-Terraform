using UnityEngine;
using System.Threading.Tasks;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

namespace Features.Biomes.UnityIntegration
{
    /// <summary>
    /// Асинхронная генерация мешей чанка, не трогает Unity API внутри Task.Run.
    /// </summary>
    public static class AsyncChunkMeshGenerator
    {
        public static async Task<Mesh> GenerateAsync(
            Vector2Int coord,
            int chunkSize,
            int resolution,
            WorldConfig world,
            bool useLowPoly
        )
        {
            Vector3 chunkOffset = new Vector3(
                coord.x * chunkSize,
                0,
                coord.y * chunkSize
            );

            return await Task.Run(() =>
            {
                try
                {
                    return TerrainMeshGenerator.GenerateMeshSync(
                        coord,
                        chunkSize,
                        resolution,
                        world,
                        useLowPoly
                    );
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AsyncChunkMeshGenerator] Exception: {ex}");
                    return null;
                }
            });
        }
    }
}
