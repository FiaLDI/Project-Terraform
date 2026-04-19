using System.Collections.Generic;
using Unity.Mathematics;

public static class PlayerSpatialGrid
{
    private static readonly Dictionary<int2, List<int>> grid = new();
    private static readonly List<int> tempResult = new(64);

    public static float CellSize = 12f;

    // ================= CLEAR =================
    public static void Clear()
    {
        foreach (var kv in grid)
            kv.Value.Clear();

        grid.Clear();
    }

    // ================= ADD =================
    public static void Add(int id, float3 pos)
    {
        int2 cell = ToCell(pos);

        if (!grid.TryGetValue(cell, out var list))
        {
            list = new List<int>(8);
            grid[cell] = list;
        }

        list.Add(id);
    }

    // ================= QUERY =================
    public static List<int> GetNearby(float3 pos)
    {
        tempResult.Clear();

        int2 center = ToCell(pos);

        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
        {
            int2 c = new int2(center.x + x, center.y + z);

            if (grid.TryGetValue(c, out var list))
                tempResult.AddRange(list);
        }

        return tempResult;
    }

    private static int2 ToCell(float3 pos)
    {
        return new int2(
            (int)math.floor(pos.x / CellSize),
            (int)math.floor(pos.z / CellSize)
        );
    }
}
