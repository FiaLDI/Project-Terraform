using UnityEngine;

namespace Features.Biomes.Application
{
    /// <summary>
    /// Чистая структура данных меша — без UnityEngine.Object.
    /// Может вычисляться в любом потоке.
    /// </summary>
    public struct MeshData
    {
        public Vector3[] vertices;
        public Vector2[] uv;
        public int[] triangles;
        public Vector3[] normals; // null — значит нужно RecalculateNormals()

        public MeshData(int vertexCount, int triCount)
        {
            vertices  = new Vector3[vertexCount];
            uv        = new Vector2[vertexCount];
            triangles = new int[triCount];
            normals   = null;
        }
    }
}
