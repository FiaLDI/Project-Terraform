using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimpleMeshMergeTool
{
    [MenuItem("Tools/Merge Selected Meshes")]
    private static void MergeSelectedMeshes()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Merge Meshes", "Выдели хотя бы один объект.", "OK");
            return;
        }

        List<MeshFilter> meshFilters = new List<MeshFilter>();
        Material sharedMaterial = null;

        foreach (GameObject go in selectedObjects)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            MeshRenderer mr = go.GetComponent<MeshRenderer>();

            if (mf == null || mr == null || mf.sharedMesh == null)
                continue;

            if (sharedMaterial == null)
            {
                sharedMaterial = mr.sharedMaterial;
            }
            else if (mr.sharedMaterial != sharedMaterial)
            {
                bool continueMerge = EditorUtility.DisplayDialog(
                    "Разные материалы",
                    $"У объектов разные материалы.\n" +
                    $"Этот простой инструмент назначит материал первого объекта.\n\n" +
                    $"Продолжить?",
                    "Да",
                    "Нет"
                );

                if (!continueMerge)
                    return;

                break;
            }

            meshFilters.Add(mf);
        }

        if (meshFilters.Count == 0)
        {
            EditorUtility.DisplayDialog("Merge Meshes", "Не найдено подходящих MeshFilter/MeshRenderer.", "OK");
            return;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        // Центр будущего объекта — средняя позиция выбранных объектов
        Vector3 center = Vector3.zero;
        foreach (MeshFilter mf in meshFilters)
        {
            center += mf.transform.position;
        }
        center /= meshFilters.Count;

        Matrix4x4 worldToLocal = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one).inverse;

        for (int i = 0; i < meshFilters.Count; i++)
        {
            combine[i] = new CombineInstance
            {
                mesh = meshFilters[i].sharedMesh,
                transform = worldToLocal * meshFilters[i].transform.localToWorldMatrix
            };
        }

        Mesh mergedMesh = new Mesh
        {
            name = "MergedMesh"
        };

        mergedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mergedMesh.CombineMeshes(combine, true, true);

        GameObject mergedObject = new GameObject("Merged Object");
        Undo.RegisterCreatedObjectUndo(mergedObject, "Create Merged Object");

        mergedObject.transform.position = center;
        mergedObject.transform.rotation = Quaternion.identity;
        mergedObject.transform.localScale = Vector3.one;

        MeshFilter mergedMF = mergedObject.AddComponent<MeshFilter>();
        MeshRenderer mergedMR = mergedObject.AddComponent<MeshRenderer>();

        mergedMF.sharedMesh = mergedMesh;
        mergedMR.sharedMaterial = sharedMaterial;

        Selection.activeGameObject = mergedObject;

        EditorUtility.DisplayDialog(
            "Merge Meshes",
            $"Готово. Объединено мешей: {meshFilters.Count}",
            "OK"
        );
    }
}
