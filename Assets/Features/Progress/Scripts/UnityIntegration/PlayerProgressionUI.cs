using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.UI;

[DisallowMultipleComponent]
public sealed class PlayerProgressionUI : PlayerBoundUIView
{
    [Header("Bindings")]
    [SerializeField] private RectTransform progressionPanel;
    [SerializeField] private Image progressionFill;
    [SerializeField] private TMP_Text progressionLevelText;
    [SerializeField] private TMP_Text progressionValueText;

    private PlayerProgressService progressService;
    private bool loggedMissingBindings;

    protected override void OnPlayerBound(GameObject player)
    {
        BindProgressService();
        RefreshProgressionHud();
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        UnbindProgressService();

        if (progressionPanel != null)
            progressionPanel.gameObject.SetActive(false);
    }

    private void BindProgressService()
    {
        UnbindProgressService();
        progressService = PlayerProgressService.Instance;
        if (progressService != null)
            progressService.ActiveCharacterChanged += HandleActiveCharacterChanged;
    }

    private void UnbindProgressService()
    {
        if (progressService != null)
            progressService.ActiveCharacterChanged -= HandleActiveCharacterChanged;

        progressService = null;
    }

    private void HandleActiveCharacterChanged(PlayerCharacterState state)
    {
        RefreshProgressionHud(state);
    }

    private void RefreshProgressionHud()
    {
        if (progressService == null)
            BindProgressService();

        RefreshProgressionHud(progressService != null ? progressService.GetActiveCharacter() : null);
    }

    private void RefreshProgressionHud(PlayerCharacterState state)
    {
        if (!HasBindings())
            return;

        if (state == null)
        {
            progressionPanel.gameObject.SetActive(false);
            return;
        }

        progressionPanel.gameObject.SetActive(true);

        int requiredExperience = PlayerProgressionRules.GetRequiredExperienceForLevel(state.level);
        float progress01 = PlayerProgressionRules.GetProgress01(state.level, state.experience);

        if (progressionFill != null)
            progressionFill.fillAmount = progress01;

        if (progressionLevelText != null)
            progressionLevelText.text = $"LVL {state.level}";

        if (progressionValueText != null)
            progressionValueText.text = $"XP {state.experience}/{requiredExperience}";
    }

    private bool HasBindings()
    {
        bool ok =
            progressionPanel != null &&
            progressionFill != null &&
            progressionLevelText != null &&
            progressionValueText != null;

        if (!ok && !loggedMissingBindings)
        {
            Debug.LogWarning("[PlayerProgressionUI] Missing serialized bindings", this);
            loggedMissingBindings = true;
        }

        return ok;
    }
}
