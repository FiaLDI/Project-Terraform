using System.Collections.Generic;
using UnityEngine;

public struct EnemyInstance
{
    public Vector3 position;
    public Quaternion rotation;
    public float scale;
    public Color color;
}

public class EnemyGPUInstancer : MonoBehaviour
{
    public static EnemyGPUInstancer Instance { get; private set; }

    [Header("Batch Settings")]
    public int batchSize = 1023;

    private readonly Dictionary<Mesh, Dictionary<Material, List<(EnemyInstance inst,
        UnityEngine.Rendering.ShadowCastingMode castShadows, bool receiveShadows, int layer)>>> _buckets
        = new();

    private Matrix4x4[] _matrices;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _matrices = new Matrix4x4[batchSize];
    }

    public void AddInstance(
        Mesh mesh,
        Material mat,
        EnemyInstance inst,
        UnityEngine.Rendering.ShadowCastingMode castShadows,
        bool receiveShadows,
        int layer)
    {
        if (mesh == null || mat == null)
            return;

        // если материал НЕ поддерживает instancing — просто выходим, без ошибок
        if (!mat.enableInstancing)
            return;

        if (!_buckets.TryGetValue(mesh, out var matDict))
        {
            matDict = new Dictionary<Material, List<(EnemyInstance, UnityEngine.Rendering.ShadowCastingMode, bool, int)>>();
            _buckets[mesh] = matDict;
        }

        if (!matDict.TryGetValue(mat, out var list))
        {
            list = new List<(EnemyInstance, UnityEngine.Rendering.ShadowCastingMode, bool, int)>(256);
            matDict[mat] = list;
        }

        list.Add((inst, castShadows, receiveShadows, layer));
    }

    private void LateUpdate()
    {
        foreach (var meshKV in _buckets)
        {
            Mesh mesh = meshKV.Key;
            var matDict = meshKV.Value;

            foreach (var matKV in matDict)
            {
                Material mat = matKV.Key;
                var list = matKV.Value;

                if (!mat.enableInstancing)
                {
                    list.Clear();
                    continue;
                }

                int total = list.Count;
                int index = 0;

                while (index < total)
                {
                    int count = Mathf.Min(batchSize, total - index);

                    for (int i = 0; i < count; i++)
                    {
                        var data = list[index + i];
                        var inst = data.inst;
                        _matrices[i] = Matrix4x4.TRS(
                            inst.position,
                            inst.rotation,
                            Vector3.one * inst.scale
                        );
                    }

                    // для простоты: одинаковые тени/слой внутри одного батча
                    var first = list[index];

                    Graphics.DrawMeshInstanced(
                        mesh,
                        0,
                        mat,
                        _matrices,
                        count,
                        null,
                        first.castShadows,
                        first.receiveShadows,
                        first.layer
                    );

                    index += count;
                }

                list.Clear();
            }
        }
    }
}
