using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;
using Features.Pooling;
using Features.Biomes.Application;
using Features.Biomes.Application.Spawning;

/// <summary>
/// Дебаг-оверлей для мониторинга:
/// - количества чанков
/// - Mesh-объектов
/// - объектов в пуле
/// - MegaSpawn задач
/// - нативной памяти
/// - FPS / frametime
/// </summary>
public class WorldDebugHUD : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F3;
    public bool visible = true;

    private float fps;
    private float deltaAccum;
    private int deltaCount;

    private GUIStyle style;
    private Rect rect;

    // ==== ПРОФАЙЛЕРЫ ====
    private static readonly ProfilerRecorder meshMemory =
        ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");

    private static readonly ProfilerRecorder colliderMemory =
        ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Physics Colliders");

    private static readonly ProfilerRecorder totalMemory =
        ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");

    void Awake()
    {
        style = new GUIStyle()
        {
            fontSize = 16,
            normal = new GUIStyleState() { textColor = Color.white }
        };

        rect = new Rect(20, 20, 600, 800);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            visible = !visible;

        // FPS
        deltaAccum += Time.unscaledDeltaTime;
        deltaCount++;

        if (deltaAccum >= 0.5f)
        {
            fps = deltaCount / deltaAccum;
            deltaAccum = 0f;
            deltaCount = 0;
        }
    }

    void OnGUI()
    {
        if (!visible)
            return;

        GUILayout.BeginArea(rect);
        GUILayout.BeginVertical("box");

        GUILayout.Label($"<b>World Debug HUD</b>", style);

        GUILayout.Space(5);
        DrawFPS();
        GUILayout.Space(5);
        DrawChunkStats();
        GUILayout.Space(5);
        DrawMeshStats();
        GUILayout.Space(5);
        DrawPoolStats();
        GUILayout.Space(5);
        DrawMegaSpawnStats();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    // ============ FPS ============
    private void DrawFPS()
    {
        GUILayout.Label(
            $"FPS: <b>{fps:0}</b>   Frame: {Time.deltaTime * 1000f:0.0} ms",
            style
        );
    }

    // ============ CHUNK STATS ============
    private void DrawChunkStats()
    {
        // Количество чанков узнаём через GetActiveChunks — ChunkManager передаёт туда список.
        int activeChunks = WorldDebugRegistry.ActiveChunkCount;
        int loadedChunks = WorldDebugRegistry.LoadedChunkCount;

        GUILayout.Label(
            $"Chunks: active={activeChunks}, loaded={loadedChunks}",
            style
        );
    }

    // ============ MESH STATS ============
    private void DrawMeshStats()
    {
        long meshMem = meshMemory.LastValue;
        long colMem = colliderMemory.LastValue;
        long totalMem = totalMemory.LastValue;

        GUILayout.Label($"Mesh Memory:  {(meshMem / (1024 * 1024))} MB", style);
        GUILayout.Label($"Collider Mem: {(colMem / (1024 * 1024))} MB", style);
        GUILayout.Label($"Total Used:   {(totalMem / (1024 * 1024))} MB", style);
    }

    // ============ POOL STATS ============
    private void DrawPoolStats()
    {
        if (SmartPool.Instance == null)
        {
            GUILayout.Label("Pool: <not initialized>", style);
            return;
        }

        GUILayout.Label("<b>Object Pool:</b>", style);

        foreach (var kv in SmartPool.Instance.Debug_GetPoolCounts())
        {
            GUILayout.Label($"  prefab {kv.Key}: {kv.Value} stored", style);
        }
    }

    // ============ MEGA SPAWN ============
    private void DrawMegaSpawnStats()
    {
        if (MegaSpawnScheduler.Instance == null)
        {
            GUILayout.Label("MegaSpawn: <not running>", style);
            return;
        }

        GUILayout.Label(
            $"MegaSpawn tasks: {MegaSpawnScheduler.Instance.Debug_ActiveTaskCount}",
            style
        );
    }
}


// ======================================================================
// === ВСПОМОГАТЕЛЬНЫЙ ГЛОБАЛЬНЫЙ РЕЕСТР (ChunkManager пишет сюда) ======
// ======================================================================

public static class WorldDebugRegistry
{
    public static int ActiveChunkCount = 0;
    public static int LoadedChunkCount = 0;
    public static int LoadQueueLength = 0;
}
