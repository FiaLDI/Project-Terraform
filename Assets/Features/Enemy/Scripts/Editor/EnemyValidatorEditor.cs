using UnityEngine;
using UnityEditor;
using Features.Enemy.Data;

[CustomEditor(typeof(EnemyConfigSO))]
public class EnemyConfigSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EnemyConfigSO config = (EnemyConfigSO)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        if (config.prefab == null)
        {
            EditorGUILayout.HelpBox("Prefab is missing!", MessageType.Error);
            return;
        }

        ValidateStructure(config.prefab);
    }

    private void ValidateStructure(GameObject prefab)
    {
        Transform modelRoot = prefab.transform.Find("Model");
        if (!modelRoot)
        {
            EditorGUILayout.HelpBox("Model root not found! (expected: 'Model')", MessageType.Error);
            return;
        }

        CheckLOD(modelRoot, "Model_LOD0", true);
        CheckLOD(modelRoot, "Model_LOD1", false);
        CheckLOD(modelRoot, "Model_LOD2", false);

        // Canvas check
        var canvas = prefab.transform.Find("Canvas");
        if (!canvas)
            EditorGUILayout.HelpBox("Canvas object not found!", MessageType.Warning);
    }

    private void CheckLOD(Transform root, string name, bool required)
    {
        Transform lod = root.Find(name);
        if (!lod)
        {
            if (required)
                EditorGUILayout.HelpBox($"{name} missing!", MessageType.Error);
            else
                EditorGUILayout.HelpBox($"{name} missing (optional).", MessageType.Warning);

            return;
        }

        var renderer = lod.GetComponentInChildren<Renderer>();
        if (!renderer)
            EditorGUILayout.HelpBox($"{name} exists but has NO Renderer!", MessageType.Error);
        else
            EditorGUILayout.LabelField($"{name}: OK ({renderer.GetType().Name})");
    }
}
