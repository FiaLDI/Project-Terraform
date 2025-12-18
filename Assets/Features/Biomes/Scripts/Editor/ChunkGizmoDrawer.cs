using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ChunkGizmoDrawer
{
    static ChunkGizmoDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView view)
    {
        Handles.color = new Color(0f, 0.8f, 1f, 0.5f);

        var chunks = Object.FindObjectsByType<ChunkRootMarker>(FindObjectsSortMode.None);

        foreach (var chunk in chunks)
        {
            Vector3 pos = chunk.transform.position;
            Vector3 size = new Vector3(32, 0, 32);

            Handles.DrawWireCube(pos + size / 2, size);
            Handles.Label(pos + Vector3.up * 2f, chunk.name);
        }
    }
}
