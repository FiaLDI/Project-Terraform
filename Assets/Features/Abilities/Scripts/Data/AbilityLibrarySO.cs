using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;

[CreateAssetMenu(
    fileName = "AbilityLibrary",
    menuName = "Game/Abilities/Ability Library")]
public sealed class AbilityLibrarySO : ScriptableObject
{
    [SerializeField] private AbilitySO[] abilities;

    private Dictionary<string, AbilitySO> _byId;

    private void OnEnable()
    {
        BuildCache();
        ValidateHandlers();
    }

    private void BuildCache()
    {
        _byId = new Dictionary<string, AbilitySO>();

        if (abilities == null)
            return;

        foreach (var ab in abilities)
        {
            if (ab == null || string.IsNullOrEmpty(ab.id))
                continue;

            if (_byId.ContainsKey(ab.id))
            {
                Debug.LogWarning(
                    $"[AbilityLibrary] Duplicate id '{ab.id}'", this);
                continue;
            }

            _byId.Add(ab.id, ab);
        }
    }

    private void ValidateHandlers()
    {
        var missing = new List<string>();

        foreach (var ab in abilities)
        {
            if (ab == null)
                continue;

            if (!AbilityExecutorHasHandler(ab))
                missing.Add($"{ab.name} ({ab.GetType().Name})");
        }

        if (missing.Count > 0)
        {
            Debug.LogError(
                "[AbilityLibrary] Missing AbilityHandler for:\n" +
                string.Join("\n", missing),
                this
            );
        }
    }

    private bool AbilityExecutorHasHandler(AbilitySO ability)
    {
        // Без прямой зависимости от AbilityExecutor.Instance
        var abilityType = ability.GetType();

        foreach (var handler in AbilityHandlerRegistry.All)
        {
            if (handler.AbilityType == abilityType)
                return true;
        }

        return false;
    }

    public AbilitySO FindById(string id)
    {
        if (string.IsNullOrEmpty(id) || _byId == null)
            return null;

        return _byId.TryGetValue(id, out var ab) ? ab : null;
    }
}
