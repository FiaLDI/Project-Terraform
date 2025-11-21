using UnityEngine;

public class InfiniteWaterPlane : MonoBehaviour
{
    public float seaLevel = 1f;
    public float planeSize = 600f;
    public Material waterMaterial;

    private MeshRenderer mr;
    private MeshFilter mf;

    void Start()
    {
        GeneratePlane();
    }

    void LateUpdate()
    {
        if (RuntimeWorldGenerator.PlayerInstance == null)
            return;

        Vector3 p = RuntimeWorldGenerator.PlayerInstance.transform.position;
        transform.position = new Vector3(p.x, seaLevel, p.z);
    }

    private void GeneratePlane()
    {
        mf = gameObject.AddComponent<MeshFilter>();
        mr = gameObject.AddComponent<MeshRenderer>();

        mr.sharedMaterial = waterMaterial;

        Mesh m = new Mesh();
        float h = planeSize;

        m.vertices = new[]
        {
            new Vector3(-h, 0, -h),
            new Vector3(-h, 0,  h),
            new Vector3( h, 0,  h),
            new Vector3( h, 0, -h)
        };

        m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();

        mf.sharedMesh = m;
    }
}
