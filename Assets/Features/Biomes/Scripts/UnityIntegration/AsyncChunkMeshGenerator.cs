using UnityEngine;
using System.Threading.Tasks;
using Features.Biomes.Domain;

namespace Features.Biomes.UnityIntegration
{
    /// <summary>
    /// Асинхронная генерация мешей чанка.
    /// Работает поверх новой версии TerrainMeshGenerator.
    /// Используется для того, чтобы не блокировать главный поток Unity.
    /// </summary>
    public static class AsyncChunkMeshGenerator
    {
        /// <summary>
        /// Генерирует Mesh для чанка асинхронно (Task.Run).
        /// Можно использовать в RuntimeWorldGenerator или ChunkManager.
        /// </summary>
        public static async Task<Mesh> GenerateAsync(
            Vector2Int coord,
            int chunkSize,
            int resolution,
            WorldConfig world,
            bool useLowPoly
        )
        {
            // ⚠ Важно: внутри Task.Run никакого Unity API!
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
