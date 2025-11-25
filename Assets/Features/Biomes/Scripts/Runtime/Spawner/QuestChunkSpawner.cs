using UnityEngine;
using Quests;

public class QuestChunkSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    public QuestChunkSpawner(Vector2Int coord, int chunkSize, BiomeConfig biome, Transform parent)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
    }

    public void Spawn()
    {
        if (biome.possibleQuests == null || biome.possibleQuests.Length == 0)
            return;

        System.Random prng = new System.Random(coord.x * 92837111 ^ coord.y * 1234567);

        foreach (var entry in biome.possibleQuests)
        {
            if (entry.questAsset == null || entry.questPointPrefab == null) continue;

            if (prng.NextDouble() > entry.spawnChance)
                continue;

            int targetsCount = prng.Next(entry.minTargets, entry.maxTargets + 1);

            for (int i = 0; i < targetsCount; i++)
            {
                float px = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
                float pz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

                float h = BiomeHeightUtility.GetHeight(biome, px, pz);
                Vector3 pos = new Vector3(px, h + 0.5f, pz);

                GameObject pointObj = Object.Instantiate(entry.questPointPrefab, pos, Quaternion.identity, parent);

                QuestPoint qp = pointObj.GetComponent<QuestPoint>();
                if (qp != null)
                {
                    qp.linkedQuest = entry.questAsset;

                    if (qp.linkedQuest.behaviour is ApproachPointQuestBehaviour approach)
                        approach.targetPoint = qp.transform;
                    else if (qp.linkedQuest.behaviour is StandOnPointQuestBehaviour stand)
                        stand.targetPoint = qp.transform;
                }
            }
        }
    }
}
