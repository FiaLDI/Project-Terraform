using Features.Input;
using Features.Player.UnityIntegration;
using Features.Player.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.UI;

public sealed class DeathScreenUI : PlayerBoundUIView, IUIScreen
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timerText;

    private NetworkPlayer networkPlayer;
    private float localDeathStartedAt;
    private bool wasDead;

    public InputMode Mode => InputMode.Disabled;

    protected override void OnEnable()
    {
        EnsureBuilt();
        base.OnEnable();
        UIRegistry.I?.Register(this);
    }

    protected override void OnDisable()
    {
        UIRegistry.I?.Unregister(this);
        base.OnDisable();
    }

    protected override void OnPlayerBound(GameObject player)
    {
        EnsureBuilt();

        networkPlayer = player != null ? player.GetComponent<NetworkPlayer>() : null;
        wasDead = networkPlayer != null && networkPlayer.IsDead;

        if (wasDead)
        {
            localDeathStartedAt = Time.unscaledTime;
            Open();
            RefreshTexts();
        }
        else
        {
            CloseIfOpen();
        }
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        networkPlayer = null;
        wasDead = false;
        CloseIfOpen();
    }

    private void Update()
    {
        if (networkPlayer == null)
        {
            CloseIfOpen();
            return;
        }

        bool isDead = networkPlayer.IsDead;

        if (isDead && !wasDead)
        {
            wasDead = true;
            localDeathStartedAt = Time.unscaledTime;
            Open();
        }
        else if (!isDead && wasDead)
        {
            wasDead = false;

            if (UIStackManager.I != null && UIStackManager.I.IsTop<DeathScreenUI>())
                UIStackManager.I.Pop();
            else
                Hide();

            return;
        }

        if (isDead)
            RefreshTexts();
    }

    public void Show()
    {
        EnsureBuilt();
        root.SetActive(true);
        RefreshTexts();
    }

    public void Hide()
    {
        EnsureBuilt();
        root.SetActive(false);
    }

    public void Open()
    {
        EnsureBuilt();

        if (UIStackManager.I == null)
        {
            Show();
            return;
        }

        if (UIStackManager.I.IsTop<DeathScreenUI>())
        {
            Show();
            return;
        }

        UIStackManager.I.Push(this);
    }

    private void CloseIfOpen()
    {
        if (UIStackManager.I != null && UIStackManager.I.IsTop<DeathScreenUI>())
            UIStackManager.I.Pop();
        else
            Hide();
    }

    private void RefreshTexts()
    {
        if (titleText != null)
            titleText.text = "YOU ARE DEAD";

        if (timerText == null)
            return;

        if (networkPlayer == null || !networkPlayer.IsDead)
        {
            timerText.text = string.Empty;
            return;
        }

        float elapsed = Time.unscaledTime - localDeathStartedAt;
        float remaining = Mathf.Max(0f, networkPlayer.RespawnDelay - elapsed);
        timerText.text = $"Respawn in {Mathf.CeilToInt(remaining)}";
    }

    private void EnsureBuilt()
    {
        if (root == null)
            root = CreateRoot();

        if (titleText == null)
            titleText = CreateLabel("Title", new Vector2(0f, 80f), 54f, FontStyles.Bold);

        if (timerText == null)
            timerText = CreateLabel("Timer", new Vector2(0f, -10f), 30f, FontStyles.Normal);

        root.SetActive(false);
    }

    private GameObject CreateRoot()
    {
        var go = new GameObject("DeathOverlay");
        go.transform.SetParent(transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.05f, 0.01f, 0.01f, 0.72f);

        var canvasGroup = go.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        return go;
    }

    private TMP_Text CreateLabel(string objectName, Vector2 anchoredPos, float fontSize, FontStyles style)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(root.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 120f);
        rect.anchoredPosition = anchoredPos;

        var text = go.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.text = string.Empty;

        return text;
    }
}
