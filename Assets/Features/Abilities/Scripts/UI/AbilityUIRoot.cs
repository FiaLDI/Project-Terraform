using UnityEngine;

public class AbilityUIRoot : MonoBehaviour
{
    public AbilitySlotUI[] slots;
    public AbilityCaster caster;
    public ClassManager classManager;

    private void Start()
    {
        if (caster == null) caster = FindAnyObjectByType<AbilityCaster>();
        if (classManager == null) classManager = FindAnyObjectByType<ClassManager>();

        Debug.Log($"AbilityUIRoot Start: caster={caster}, classManager={classManager}");

        if (classManager != null)
        {
            classManager.OnAbilitiesChanged += RefreshSlots;
            Debug.Log("Subscribed to OnAbilitiesChanged");
        }

        RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (classManager != null && classManager.ActiveAbilities != null)
        {
            Debug.Log($"Refreshing {slots.Length} slots, ActiveAbilities length: {classManager.ActiveAbilities.Length}");

            for (int i = 0; i < slots.Length; i++)
            {
                AbilitySO ability = null;
                if (i < classManager.ActiveAbilities.Length)
                {
                    ability = classManager.ActiveAbilities[i];
                }

                Debug.Log($"Slot {i}: {ability?.name ?? "NULL"}");
                if (slots[i] != null)
                {
                    slots[i].Bind(ability, caster, i);
                }
            }
        }
        else
        {
            Debug.LogWarning("ClassManager or ActiveAbilities is null, will retry...");
            Invoke(nameof(RefreshSlots), 0.1f);
        }
    }

    private void OnDestroy()
    {
        if (classManager != null)
        {
            classManager.OnAbilitiesChanged -= RefreshSlots;
        }
    }
}