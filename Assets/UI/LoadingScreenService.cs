using System.Collections.Generic;
using System.Collections;
using Biomes.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingScreenService : MonoBehaviour
{
    public static LoadingScreenService I { get; private set; }

    private static Sprite sharedHubBackground;

    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField, Min(0f)] private float hideDelay = 1f;

    [Header("Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite hubBackground;
    [SerializeField] private Sprite fallbackBackground;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subtitle;

    private static readonly Dictionary<string, Sprite> worldBackgrounds = new();
    private static PendingState pending;
    private Coroutine hideRoutine;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        if (hubBackground == null)
            hubBackground = sharedHubBackground;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(transform.root.gameObject);

        if (root != null)
            root.SetActive(false);

        ApplyPendingIfNeeded();
    }

    private void OnDestroy()
    {
        if (I == this)
            I = null;
    }

    public static void SetHubBackground(Sprite sprite)
    {
        sharedHubBackground = sprite;

        if (I != null)
            I.hubBackground = sprite;
    }

    public static void RegisterWorldBackground(WorldConfig world)
    {
        if (world == null || string.IsNullOrWhiteSpace(world.name) || world.loadingBackground == null)
            return;

        worldBackgrounds[world.name] = world.loadingBackground;
    }

    public static void ShowHub(string subtitleText = "Preparing hub...")
    {
        Sprite background = I != null ? I.hubBackground : sharedHubBackground;
        Show("Loading hub", subtitleText, background);
    }

    public static void ShowWorld(WorldConfig world, string subtitleText = "Preparing procedural world...")
    {
        string worldName = world != null ? world.name : "world";
        Show($"Loading {worldName}", subtitleText, world != null ? world.loadingBackground : null);
    }

    public static void ShowWorld(string worldConfigId, string subtitleText = "Preparing procedural world...")
    {
        Sprite background = null;
        if (!string.IsNullOrWhiteSpace(worldConfigId))
            worldBackgrounds.TryGetValue(worldConfigId, out background);

        string worldName = string.IsNullOrWhiteSpace(worldConfigId) ? "world" : worldConfigId;
        Show($"Loading {worldName}", subtitleText, background);
    }

    public static void Show(string titleText = "Loading", string subtitleText = "", Sprite background = null)
    {
        pending = new PendingState(titleText, subtitleText, background, true);

        if (I == null)
        {
            Debug.LogWarning("[LoadingScreen] No LoadingScreenService instance in scene.");
            return;
        }

        I.Apply(titleText, subtitleText, background, true);
    }

    public static void Hide()
    {
        pending = new PendingState(string.Empty, string.Empty, null, false);

        if (I == null)
            return;

        I.HideWithDelay();
    }

    public static void SetMessage(string titleText, string subtitleText = "")
    {
        if (I == null)
            return;

        I.SetText(titleText, subtitleText);
    }

    private void ApplyPendingIfNeeded()
    {
        if (!pending.Initialized)
            return;

        Apply(pending.Title, pending.Subtitle, pending.Background, pending.Visible);
    }

    private void Apply(string titleText, string subtitleText, Sprite background, bool visible)
    {
        if (visible && hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (visible)
        {
            SetText(titleText, subtitleText);
            SetBackground(background);
        }

        if (root != null)
            root.SetActive(visible);
    }

    private void HideWithDelay()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        if (hideDelay <= 0f)
        {
            Apply(string.Empty, string.Empty, null, false);
            return;
        }

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(hideDelay);
        hideRoutine = null;
        Apply(string.Empty, string.Empty, null, false);
    }

    private void SetText(string titleText, string subtitleText)
    {
        if (title != null)
            title.text = titleText;

        if (subtitle != null)
            subtitle.text = subtitleText;
    }

    private void SetBackground(Sprite sprite)
    {
        if (backgroundImage == null)
            return;

        Sprite resolved = sprite != null ? sprite : fallbackBackground;
        backgroundImage.sprite = resolved;
        backgroundImage.enabled = resolved != null;
    }

    private readonly struct PendingState
    {
        public readonly string Title;
        public readonly string Subtitle;
        public readonly Sprite Background;
        public readonly bool Visible;
        public readonly bool Initialized;

        public PendingState(string title, string subtitle, Sprite background, bool visible)
        {
            Title = title;
            Subtitle = subtitle;
            Background = background;
            Visible = visible;
            Initialized = true;
        }
    }
}
