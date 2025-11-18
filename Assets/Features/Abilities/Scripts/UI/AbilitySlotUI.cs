using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
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
        this.ability = ability;
        this.caster = caster;
        this.index = index;

        if (keyLabel)
            keyLabel.text = (index + 1).ToString();

        if (ability == null)
        {
            if (icon != null)
                icon.sprite = defaultIcon;

            if (cooldownSlider != null)
            {
                cooldownSlider.minValue = 0;
                cooldownSlider.maxValue = 1;  
                cooldownSlider.value = 1;
            }
            return;
        }
        if (icon != null)
            icon.sprite = ability.icon;

        if (cooldownSlider != null)
        {
            cooldownSlider.minValue = 0;
            cooldownSlider.maxValue = ability.cooldown;
            cooldownSlider.value = ability.cooldown;
        }

        caster.OnAbilityCast += HandleCastReset;
        caster.OnCooldownChanged += HandleCooldownUpdate;
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
}
