using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Profiling;

public class WorldDebugHUDCanvas : MonoBehaviour
{
    public bool visible = true;

    private TextMeshProUGUI text;
    private Canvas canvas;

    // Profilers
    private static readonly ProfilerRecorder meshMemory =
        ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");

    private static readonly ProfilerRecorder colliderMemory =
        ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Physics Colliders");

    private static readonly ProfilerRecorder totalMemory =
        ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");

    private float fps;
    private float accum;
    private int frames;

    void Start()
    {
        // Create Canvas safely
        var canvasGO = new GameObject("DebugCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(canvasGO);

        var textGO = new GameObject("DebugText");
        textGO.transform.SetParent(canvasGO.transform);

        text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = Color.white;

        RectTransform rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(600, 800);
    }

    void Update()
    {
        // ========== SAFE INPUT (NEW INPUT SYSTEM) ==========
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            visible = !visible;
            if (text != null)
                text.enabled = visible;
        }

        if (!visible)
            return;

        // FPS
        accum += Time.unscaledDeltaTime;
        frames++;

        if (accum > 0.5f)
        {
            fps = frames / accum;
            accum = 0;
            frames = 0;
        }

        if (text == null || canvas == null)
            return;

        long meshMem = meshMemory.LastValue / (1024 * 1024);
        long colMem = colliderMemory.LastValue / (1024 * 1024);
        long totMem = totalMemory.LastValue / (1024 * 1024);

        text.text =
            $"<b>WORLD DEBUG HUD</b>\n" +
            $"FPS: {fps:0}   ({Time.deltaTime * 1000f:0.0} ms)\n\n" +

            $"<b>Chunks</b>\n" +
            $"Active: {WorldDebugRegistry.ActiveChunkCount}\n" +
            $"Loaded: {WorldDebugRegistry.LoadedChunkCount}\n" +
            $"LoadQueue: {WorldDebugRegistry.LoadQueueLength}\n\n" +

            $"<b>Memory</b>\n" +
            $"Mesh: {meshMem} MB\n" +
            $"Colliders: {colMem} MB\n" +
            $"Total Used: {totMem} MB\n";
    }
}
