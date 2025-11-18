using UnityEngine;

public class AbilityUIRoot : MonoBehaviour
{
    public AbilitySlotUI[] slots;
    public AbilityCaster caster;
    public ClassManager classManager;

    private void Start()
    {
        if (caster == null) caster = FindAnyObjectByType<AbilityCaster>();
        if (classManager == null) classManager = caster.GetComponent<ClassManager>();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Bind(classManager.ActiveAbilities[i], caster, i);
        }
    }
}
