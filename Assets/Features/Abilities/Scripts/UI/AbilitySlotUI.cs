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
        
        Debug.Log($"AbilitySlotUI Awake: icon={icon}, cooldownSlider={cooldownSlider}, keyLabel={keyLabel}");
    }

    public void Bind(AbilitySO ability, AbilityCaster caster, int index)
    {
        Debug.Log($"AbilitySlotUI.Bind: slot {index}, ability={ability?.name ?? "NULL"}, icon={(ability != null ? ability.icon : "NULL")}");

        Unsubscribe();

        this.ability = ability;
        this.caster = caster;
        this.index = index;

        if (keyLabel != null)
        {
            keyLabel.text = (index + 1).ToString();
            Debug.Log($"Set keyLabel to: {keyLabel.text}");
        }

        if (ability == null)
        {
            Debug.Log($"Ability is null, setting default icon");
            if (icon != null)
            {
                icon.sprite = defaultIcon;
                icon.color = Color.gray; // Делаем серым для пустого слота
            }

            if (cooldownSlider != null)
            {
                cooldownSlider.minValue = 0;
                cooldownSlider.maxValue = 1;  
                cooldownSlider.value = 1;
            }
            return;
        }

        // Ability не null
        Debug.Log($"Setting ability: {ability.name}, icon={ability.icon}");

        if (icon != null)
        {
            if (ability.icon != null)
            {
                icon.sprite = ability.icon;
                icon.color = Color.white; // Нормальный цвет
                Debug.Log($"Icon sprite set to: {ability.icon.name}");
            }
            else
            {
                Debug.LogWarning($"Ability {ability.name} has no icon assigned!");
                icon.sprite = defaultIcon;
                icon.color = Color.yellow; // Желтый для отсутствующей иконки
            }
        }
        else
        {
            Debug.LogError("Icon Image component is null!");
        }

        if (cooldownSlider != null)
        {
            cooldownSlider.minValue = 0;
            cooldownSlider.maxValue = ability.cooldown;
            cooldownSlider.value = ability.cooldown;
        }

        // Подписываемся на события только если caster существует
        if (caster != null)
        {
            caster.OnAbilityCast += HandleCastReset;
            caster.OnCooldownChanged += HandleCooldownUpdate;
            Debug.Log($"Subscribed to caster events");
        }
        else
        {
            Debug.LogError("Caster is null!");
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

        if (cooldownSlider != null)
            cooldownSlider.value = 0;
    }

    private void HandleCooldownUpdate(AbilitySO updated, float remaining, float max)
    {
        if (updated != ability) return;

        if (cooldownSlider != null)
            cooldownSlider.value = max - remaining;
    }
}