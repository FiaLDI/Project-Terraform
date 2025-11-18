using UnityEngine;
using System.Collections.Generic;

public class AbilityCaster : MonoBehaviour
{
    public Camera AimCamera;
    public LayerMask GroundMask;

    private ClassManager classManager;
    private PlayerEnergy energy;

    private Dictionary<AbilitySO, float> cooldowns = new();
    public event System.Action<AbilitySO, float, float> OnCooldownChanged;
    public event System.Action<AbilitySO> OnAbilityCast;



    private void Awake()
    {
        classManager = GetComponent<ClassManager>();
        energy = GetComponent<PlayerEnergy>();
    }

    private void Update()
    {
        UpdateCooldowns();
    }

    void UpdateCooldowns()
{
    var keys = new List<AbilitySO>(cooldowns.Keys);

    foreach (var ability in keys)
    {
        if (cooldowns[ability] > 0)
        {
            cooldowns[ability] -= Time.deltaTime;

            // Событие: ability, remaining, maxCooldown
            OnCooldownChanged?.Invoke(
                ability,
                Mathf.Max(0, cooldowns[ability]),
                ability.cooldown
            );
        }
        else
        {
            cooldowns[ability] = 0;
        }
    }
}

    public float GetCooldownRemaining(AbilitySO ability)
    {
        if (!cooldowns.ContainsKey(ability)) 
            return 0;

        return Mathf.Max(0, cooldowns[ability]);
    }

    public void TryCast(int index)
    {
        if (index < 0 || index >= classManager.ActiveAbilities.Length)
            return;

        var ability = classManager.ActiveAbilities[index];
        if (ability == null) return;

        if (GetCooldownRemaining(ability) > 0) return;
        if (!energy.HasEnergy(ability.energyCost)) return;

        Vector3 point = GetTargetPoint();

        AbilityContext ctx = new AbilityContext
        {
            Owner = gameObject,
            AimCamera = AimCamera,
            TargetPoint = point,
            Direction = AimCamera ? AimCamera.transform.forward : transform.forward,
            SlotIndex = index
        };

        Debug.Log($"CAST {ability.name} | cost={ability.energyCost} | energyBefore={energy.CurrentEnergy}");

        if (!energy.TrySpend(ability.energyCost))
        {
            Debug.Log("[ENERGY] Not enough energy!");
            return;
        }

        OnAbilityCast?.Invoke(ability);

        ability.Execute(ctx);
        cooldowns[ability] = ability.cooldown;
    }
    private Vector3 GetTargetPoint()
    {
        if (AimCamera == null)
            return transform.position + transform.forward * 3f;

        Ray ray = AimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, GroundMask))
            return hit.point;

        return transform.position + transform.forward * 3f;
    }
}
