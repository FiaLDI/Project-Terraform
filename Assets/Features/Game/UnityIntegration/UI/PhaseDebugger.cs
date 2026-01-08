#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Object;
using System.Text;

public sealed class PhaseDebuggerUI : MonoBehaviour
{
    private Text text;
    private ServerGamePhase phase;
    [SerializeField] private Font debugFont;


    private bool searching = true;

    private void Awake()
    {
        CreateText();
        text.text = "PhaseDebugger: waiting for player...";
    }

    private void Update()
    {
        if (!searching)
            return;

        TryFindLocalPlayer();
    }

    private void OnDestroy()
    {
        if (phase != null)
            phase.OnPhaseReached -= OnPhaseReached;
    }

    // =====================================================
    // FIND PLAYER
    // =====================================================

    private void TryFindLocalPlayer()
    {
        if (!InstanceFinder.IsClient)
            return;

        foreach (var nob in FindObjectsOfType<NetworkObject>())
        {
            if (!nob.IsOwner)
                continue;

            var p = nob.GetComponent<ServerGamePhase>();
            if (p == null)
                continue;

            phase = p;
            searching = false;

            phase.OnPhaseReached += OnPhaseReached;

            UpdateText();

            Debug.Log("[PhaseDebuggerUI] Connected to local player phase", this);
            return;
        }
    }

    // =====================================================
    // PHASE UPDATE
    // =====================================================

    private void OnPhaseReached(GamePhase _)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (phase == null)
            return;

        var sb = new StringBuilder();
        sb.AppendLine("<b>GamePhase</b>");
        sb.AppendLine($"Current: <color=yellow>{phase.Current}</color>");

        text.text = sb.ToString();
    }

    // =====================================================
    // UI
    // =====================================================

    private void CreateText()
    {
        var go = new GameObject("PhaseDebuggerText");
        go.transform.SetParent(transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 200);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(10f, -10f);

        text = go.AddComponent<Text>();
        text.font =
    debugFont != null
        ? debugFont
        : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.supportRichText = true;
    }
}
#endif
