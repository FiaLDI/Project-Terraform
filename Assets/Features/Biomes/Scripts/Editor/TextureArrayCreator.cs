using UnityEngine;
using UnityEditor;

public class TextureArrayCreator : EditorWindow
{
    private int textureCount = 4;
    private Texture2D[] textures;
    private string savePath = "Assets/TextureArray.asset";

    [MenuItem("Tools/Create Texture2DArray")]
    public static void ShowWindow()
    {
        GetWindow<TextureArrayCreator>("Texture2DArray Creator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Texture2DArray Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        textureCount = EditorGUILayout.IntField("Textures Count:", textureCount);

        // ИНИЦИАЛИЗАЦИЯ МАССИВА — ОБЯЗАТЕЛЬНО!
        if (textures == null || textures.Length != textureCount)
        {
            textures = new Texture2D[textureCount];
        }

        EditorGUILayout.Space();

        // ВВОД ТЕКСТУР
        for (int i = 0; i < textureCount; i++)
        {
            textures[i] = (Texture2D)EditorGUILayout.ObjectField($"Texture {i}", textures[i], typeof(Texture2D), false);
        }

        EditorGUILayout.Space();

        savePath = EditorGUILayout.TextField("Save Path:", savePath);

        if (GUILayout.Button("Generate Array", GUILayout.Height(30)))
        {
            GenerateArray();
        }
    }

    private void GenerateArray()
    {
        if (textures == null || textures.Length == 0)
        {
            Debug.LogError("Texture list is empty!");
            return;
        }

        // Проверка нулевых текстур
        foreach (var tex in textures)
        {
            if (tex == null)
            {
                Debug.LogError("One or more texture slots are empty!");
                return;
            }
        }

        int size = textures[0].width;
        TextureFormat format = textures[0].format;

        // Создаем Texture2DArray
        Texture2DArray array = new Texture2DArray(size, size, textures.Length, format, true);

        for (int i = 0; i < textures.Length; i++)
        {
            Graphics.CopyTexture(textures[i], 0, 0, array, i, 0);
        }

        AssetDatabase.CreateAsset(array, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Texture2DArray saved: {savePath}");
    }
}
