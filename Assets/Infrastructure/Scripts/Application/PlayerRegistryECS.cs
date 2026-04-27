using System.Collections.Generic;
using Unity.Entities;

public static class PlayerRegistryECS
{
    private static readonly List<Entity> players = new();
    private static readonly Dictionary<int, Entity> byNetId = new();
    private static readonly Dictionary<Entity, int> byEntity = new();
    private static readonly Dictionary<int, Entity> byEntityId = new();

    public static IReadOnlyList<Entity> All => players;

    // ================= REGISTER =================
    public static void Register(int netId, Entity entity)
    {
        if (byNetId.ContainsKey(netId))
            return;

        byNetId[netId] = entity;
        byEntityId[GetKey(entity)] = entity;
        byEntity[entity] = netId;

        players.Add(entity);
    }

    // ================= UNREGISTER =================
    public static void Unregister(int netId)
    {
        if (!byNetId.TryGetValue(netId, out var entity))
            return;

        byNetId.Remove(netId);
        byEntityId.Remove(GetKey(entity));
        byEntity.Remove(entity);

        players.Remove(entity);
    }

    public static bool TryGet(int netId, out Entity entity)
        => byNetId.TryGetValue(netId, out entity);

    public static bool TryGetEntity(int entityIndex, int entityVersion, out Entity entity)
    {
        return byEntityId.TryGetValue(Hash(entityIndex, entityVersion), out entity);
    }
    public static bool TryGetNetId(Entity entity, out int netId)
        => byEntity.TryGetValue(entity, out netId);

    // ================= SAFE CLEANUP =================
    public static void Cleanup(EntityManager em)
    {
        for (int i = players.Count - 1; i >= 0; i--)
        {
            var e = players[i];

            if (!em.Exists(e))
            {
                players.RemoveAt(i);
                byEntityId.Remove(GetKey(e));

                // ищем и удаляем из netId
                foreach (var kv in byNetId)
                {
                    if (kv.Value == e)
                    {
                        byNetId.Remove(kv.Key);
                        break;
                    }
                }
            }
        }
    }

    // ================= CLEAR =================
    public static void Clear()
    {
        players.Clear();
        byNetId.Clear();
        byEntityId.Clear();
        byEntity.Clear();
    }

    // ================= INTERNAL =================
    private static int GetKey(Entity e)
        => Hash(e.Index, e.Version);

    private static int Hash(int index, int version)
        => (index * 397) ^ version;
}
