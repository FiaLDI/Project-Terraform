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

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Bind(classManager.ActiveAbilities[i], caster, i);
        }

        // обновлять энергию по событию
        energy.OnEnergyChanged += UpdateEnergyView;
    }

    private void UpdateEnergyView(float current, float max)
    {
        if (energyFill != null)
            energyFill.fillAmount = current / max;
    }
}
