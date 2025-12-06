using UnityEngine;
using UnityEditor;
using Features.Enemy.Data;
using Features.Enemy;

public class EnemyEditorWindow : EditorWindow
{
    private EnemyConfigSO config;
    private Vector2 scroll;

    [MenuItem("Game/Enemy Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnemyEditorWindow>("Enemy Editor");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Enemy Config Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        config = (EnemyConfigSO)EditorGUILayout.ObjectField(
            "Config",
            config,
            typeof(EnemyConfigSO),
            false
        );

        if (config == null)
        {
            EditorGUILayout.HelpBox("Select EnemyConfigSO to begin.", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawInfoBlock();
        DrawStats();
        DrawModelValidation();
        DrawLODSettings();
        DrawCanvasSettings();
        DrawInstancing();
        DrawBuildButtons();

        EditorGUILayout.EndScrollView();
    }

    private void DrawInfoBlock()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);

        config.enemyId = EditorGUILayout.TextField("Enemy ID", config.enemyId);
        config.displayName = EditorGUILayout.TextField("Display Name", config.displayName);
        config.icon = (Sprite)EditorGUILayout.ObjectField("Icon", config.icon, typeof(Sprite), false);

        config.prefab = (GameObject)EditorGUILayout.ObjectField(
            "Base Prefab",
            config.prefab,
            typeof(GameObject),
            false
        );

        if (config.prefab == null)
            EditorGUILayout.HelpBox("Enemy must have a base prefab!", MessageType.Error);
    }

    private void DrawStats()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        config.baseMaxHealth = EditorGUILayout.FloatField("Max Health", config.baseMaxHealth);
    }

    private void DrawModelValidation()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("LOD Validator", EditorStyles.boldLabel);

        if (config.prefab == null)
        {
            EditorGUILayout.HelpBox("Prefab not assigned!", MessageType.Error);
            return;
        }

        Transform model = config.prefab.transform.Find("Model");
        if (!model)
        {
            EditorGUILayout.HelpBox("Model root 'Model' not found!", MessageType.Error);
            return;
        }

        ValidateLOD(model, "Model_LOD0", true);
        ValidateLOD(model, "Model_LOD1", false);
        ValidateLOD(model, "Model_LOD2", false);
    }

    private void ValidateLOD(Transform root, string childName, bool required)
    {
        var child = root.Find(childName);
        if (!child)
        {
            if (required)
                EditorGUILayout.HelpBox($"{childName} missing!", MessageType.Error);
            else
                EditorGUILayout.HelpBox($"{childName} optional but missing.", MessageType.Warning);
            return;
        }

        var renderer = child.GetComponentInChildren<Renderer>();
        if (!renderer)
            EditorGUILayout.HelpBox($"{childName} found but has NO Renderer!", MessageType.Error);
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{childName}: OK ({renderer.GetType().Name})");

            Texture2D preview = AssetPreview.GetAssetPreview(renderer.gameObject);
            if (preview) GUILayout.Label(preview, GUILayout.Width(64), GUILayout.Height(64));

            EditorGUILayout.EndHorizontal();
        }
    }


    private void DrawLODSettings()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("LOD Distances", EditorStyles.boldLabel);

        config.lod0Distance = EditorGUILayout.FloatField("LOD0 Distance", config.lod0Distance);
        config.lod1Distance = EditorGUILayout.FloatField("LOD1 Distance", config.lod1Distance);
        config.lod2Distance = EditorGUILayout.FloatField("LOD2 Distance", config.lod2Distance);
    }

    private void DrawCanvasSettings()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Canvas", EditorStyles.boldLabel);

        config.worldCanvasPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Canvas Prefab",
            config.worldCanvasPrefab,
            typeof(GameObject),
            false
        );

        config.canvasHideDistance = EditorGUILayout.FloatField("Canvas Hide Distance", config.canvasHideDistance);
    }

    private void DrawInstancing()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Instancing", EditorStyles.boldLabel);

        config.useGPUInstancing = EditorGUILayout.Toggle("Use Instancing", config.useGPUInstancing);

        if (config.useGPUInstancing)
        {
            config.instancingDistance = EditorGUILayout.FloatField("Instancing Distance", config.instancingDistance);
            config.disableAnimatorInInstancing = EditorGUILayout.Toggle("Disable Animator", config.disableAnimatorInInstancing);
            config.makeRigidbodyKinematicInInstancing = EditorGUILayout.Toggle("Make Rigidbody Kinematic", config.makeRigidbodyKinematicInInstancing);
        }
    }

    private void DrawBuildButtons()
    {
        GUILayout.Space(20);

        if (GUILayout.Button("Apply Config Changes", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Generate Runtime Enemy Prefab", GUILayout.Height(30)))
        {
            GenerateFinalPrefab();
        }
    }

    private void GenerateFinalPrefab()
    {
        if (config.prefab == null)
        {
            Debug.LogError("[EnemyEditor] Cannot build enemy, base prefab missing!");
            return;
        }

        string basePath = AssetDatabase.GetAssetPath(config.prefab);
        string folder = System.IO.Path.GetDirectoryName(basePath);
        string outputPath = folder + "/" + config.displayName + "_Runtime.prefab";

        // Clone base prefab
        GameObject clone = PrefabUtility.InstantiatePrefab(config.prefab) as GameObject;

        // Add required systems
        if (!clone.GetComponent<EnemyInstanceTracker>())
            clone.AddComponent<EnemyInstanceTracker>().config = config;

        if (!clone.GetComponent<EnemyLODController>())
            clone.AddComponent<EnemyLODController>().config = config;

        if (!clone.GetComponent<EnemyHealth>())
            clone.AddComponent<EnemyHealth>().config = config;

        PrefabUtility.SaveAsPrefabAsset(clone, outputPath);
        DestroyImmediate(clone);

        Debug.Log($"[EnemyEditor] Runtime enemy prefab generated: {outputPath}");
    }
}
