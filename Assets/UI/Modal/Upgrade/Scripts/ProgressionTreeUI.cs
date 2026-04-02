using UnityEngine;
using Features.Input;
using Features.UI;
using Features.Classes.Data;
using Features.Class.Net;

public sealed class ProgressionTreeUI : PlayerBoundUIView, IUIScreen
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform container;
    [SerializeField] private ProgressionNodeView nodePrefab;

    private PlayerClassConfigSO currentClass;

    public InputMode Mode => InputMode.Dialog;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);
        root.SetActive(false);
    }

    protected override void OnDisable()
    {
        UIRegistry.I?.Unregister(this);
        base.OnDisable();
    }

    protected override void OnPlayerBound(GameObject player)
    {
        root.SetActive(false);
    }

    // =====================================================
    // SCREEN
    // =====================================================

    public void Show()
    {
        root.SetActive(true);
        InputModeManager.I.SetMode(Mode);
    }

    public void Hide()
    {
        root.SetActive(false);
        InputModeManager.I.SetMode(InputMode.Gameplay);
    }

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    public void OnCloseClicked()
    {
        UIStackManager.I.Pop();
    }

    // =====================================================
    // BUILD TREE
    // =====================================================

    public void Build(PlayerClassConfigSO cfg)
    {
        if (cfg == null || cfg.progression == null)
        {
            Debug.LogError("[ProgressionTreeUI] Missing progression config");
            return;
        }

        currentClass = cfg;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        var state = PlayerProgressService.Instance.GetActiveCharacter();

        foreach (var node in cfg.progression.nodes)
        {
            if (node == null)
                continue;

            var view = Instantiate(nodePrefab, container);

            view.transform.localPosition = node.position;

            bool unlocked = state.passives.Contains(node.passive.id);
            bool available = state.level >= node.requiredLevel;

            view.Init(
                node,
                unlocked,
                available,
                () => TryUnlock(node),
                () => TryRemove(node)
            );
        }
    }

    // =====================================================
    // UNLOCK LOGIC
    // =====================================================

    private void TryUnlock(ProgressionNodeSO node)
    {
        if (BoundPlayer == null || node == null || node.passive == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();

        if (state.level < node.requiredLevel)
            return;

        if (state.passives.Contains(node.passive.id))
            return;

        state.passives.Add(node.passive.id);
        PlayerProgressService.Instance.Save();

        var net = BoundPlayer.GetComponent<PlayerStateNetAdapter>();
        net.RefreshPassives();

        Build(currentClass);
    }

    private void TryRemove(ProgressionNodeSO node)
    {
        if (BoundPlayer == null || node == null || node.passive == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();

        if (!state.passives.Contains(node.passive.id))
            return;

        // 🔥 УДАЛЯЕМ
        state.passives.Remove(node.passive.id);

        PlayerProgressService.Instance.Save();

        // 🔥 HOT RELOAD
        var net = BoundPlayer.GetComponent<PlayerStateNetAdapter>();
        net.RefreshPassives();

        // 🔥 обновляем UI
        Build(currentClass);
    }

    public void OnResetAllClicked()
    {
        if (BoundPlayer == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();

        // 🔥 очищаем всё
        state.passives.Clear();

        PlayerProgressService.Instance.Save();

        // 🔥 hot reload
        var net = BoundPlayer.GetComponent<PlayerStateNetAdapter>();
        net.RefreshPassives();

        // 🔥 обновляем UI
        Build(currentClass);
    }
}
