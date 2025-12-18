using UnityEngine;
using Features.Biomes.Application;
using Features.Biomes.Domain;

public static class TerrainMeshGenerator
{
    public static Mesh BuildMesh(MeshData data)
    {
        Mesh m = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        m.vertices  = data.vertices;
        m.uv        = data.uv;
        m.triangles = data.triangles;

        if (data.normals != null)
            m.normals = data.normals;
        else
            m.RecalculateNormals();

        m.RecalculateBounds();
        return m;
    }

    public static Mesh GenerateMeshSync(
        Vector2Int coord,
        int chunkSize,
        int resolution,
        WorldConfig world,
        bool useLowPoly)
    {
        MeshData data = MeshDataGenerator.GenerateData(
            coord,
            chunkSize,
            resolution,
            world,
            useLowPoly
        );

        return BuildMesh(data);
    }
}
