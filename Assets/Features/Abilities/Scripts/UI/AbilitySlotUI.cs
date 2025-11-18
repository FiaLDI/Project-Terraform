using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public Image cooldownMask;
    public TextMeshProUGUI keyLabel;

    private AbilitySO ability;
    private AbilityCaster caster;
    private int index;

    public void Bind(AbilitySO ability, AbilityCaster caster, int index)
    {
        this.ability = ability;
        this.caster = caster;
        this.index = index;

        if (ability == null)
        {
            if (icon != null) icon.enabled = false;
            if (cooldownMask != null) cooldownMask.fillAmount = 0;
            return;
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = ability.icon;
        }

        if (keyLabel != null)
        {
            keyLabel.text = (index + 1).ToString();
        }
    }

    public void UpdateCooldown()
    {
        if (ability == null || caster == null || cooldownMask == null)
            return;

        float cd = caster.GetCooldownRemaining(ability);
        cooldownMask.fillAmount = cd > 0 ? (cd / ability.cooldown) : 0f;
    }
}
