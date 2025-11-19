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

        if (classManager != null && classManager.ActiveAbilities != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < classManager.ActiveAbilities.Length && classManager.ActiveAbilities[i] != null)
                {
                    slots[i].Bind(classManager.ActiveAbilities[i], caster, i);
                }
                else
                {
                    slots[i].Bind(null, caster, i);
                }
            }
        }

        if (energy != null)
            energy.OnEnergyChanged += UpdateEnergyView;
    }

    private void OnDestroy()
    {
        if (energy != null)
            energy.OnEnergyChanged -= UpdateEnergyView;
    }

    private void UpdateEnergyView(float current, float max)
    {
        if (energyFill != null && max > 0)
            energyFill.fillAmount = current / max;
    }
}