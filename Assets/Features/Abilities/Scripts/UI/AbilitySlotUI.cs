using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AbilitySlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image icon;
    public Slider cooldownSlider;
    public TextMeshProUGUI keyLabel;

    private AbilitySO ability;
    private AbilityCaster caster;
    private int index;

    private Sprite defaultIcon;

    private void Awake()
    {
        if (icon != null)
            defaultIcon = icon.sprite;
    }

    public void Bind(AbilitySO ability, AbilityCaster caster, int index)
    {
        Unsubscribe();

        this.ability = ability;
        this.caster = caster;
        this.index = index;

        if (keyLabel != null)
            keyLabel.text = (index + 1).ToString();

        if (ability == null)
        {
            if (icon != null)
            {
                icon.sprite = defaultIcon;
                icon.color = Color.gray;
            }

            if (cooldownSlider != null)
            {
                cooldownSlider.minValue = 0;
                cooldownSlider.maxValue = 1;
                cooldownSlider.value = 1;
            }
            return;
        }

        if (icon != null)
        {
            icon.sprite = ability.icon != null ? ability.icon : defaultIcon;
            icon.color = ability.icon != null ? Color.white : Color.yellow;
        }

        if (cooldownSlider != null)
        {
            cooldownSlider.minValue = 0;
            cooldownSlider.maxValue = ability.cooldown;
            cooldownSlider.value = ability.cooldown;
        }

        if (caster != null)
        {
            caster.OnAbilityCast += HandleCastReset;
            caster.OnCooldownChanged += HandleCooldownUpdate;
        }
    }

    private void Unsubscribe()
    {
        if (caster != null)
        {
            caster.OnAbilityCast -= HandleCastReset;
            caster.OnCooldownChanged -= HandleCooldownUpdate;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void HandleCastReset(AbilitySO usedAbility)
    {
        if (usedAbility != ability) return;
        cooldownSlider.value = 0;
    }

    private void HandleCooldownUpdate(AbilitySO updated, float remaining, float max)
    {
        if (updated != ability) return;
        cooldownSlider.value = max - remaining;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ability != null)
            TooltipController.Instance.ShowAbility(ability, caster);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}
