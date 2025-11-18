using UnityEngine;

public class AbilityHUD : MonoBehaviour
{
    public AbilitySlotUI[] slots;
    public PlayerEnergy energy;
    public UnityEngine.UI.Image energyFill;

    private AbilityCaster caster;
    private ClassManager classManager;

    private void Start()
    {
        caster = FindObjectOfType<AbilityCaster>();
        classManager = FindObjectOfType<ClassManager>();

        if (caster == null || classManager == null)
        {
            Debug.LogError("AbilityHUD: caster/classManager not found!");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var ability = classManager.ActiveAbilities[i];
            slots[i].Bind(ability, caster, i);
        }
    }

    private void Update()
    {
        if (energy && energyFill)
        {
            energyFill.fillAmount = energy.CurrentEnergy / energy.MaxEnergy;
        }

        // Update cooldowns for all slots
        foreach (var slot in slots)
        {
            slot.UpdateCooldown();
        }
    }
}
