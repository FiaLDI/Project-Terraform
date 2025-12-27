using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;

[CreateAssetMenu(
    fileName = "AbilityLibrary",
    menuName = "Game/Abilities/Ability Library")]
public sealed class AbilityLibrarySO : ScriptableObject
{
    [SerializeField] private AbilitySO[] abilities;

    private Dictionary<string, AbilitySO> _byId;

    void OnEnable()
    {
        _byId = new Dictionary<string, AbilitySO>();
        if (abilities == null) return;

        foreach (var ab in abilities)
        {
            if (ab == null || string.IsNullOrEmpty(ab.id))
                continue;

            if (_byId.ContainsKey(ab.id))
            {
                Debug.LogWarning($"[AbilityLibrary] Duplicate id {ab.id}", this);
                continue;
            }

            _byId.Add(ab.id, ab);
        }
    }

    public AbilitySO FindById(string id)
    {
        if (string.IsNullOrEmpty(id) || _byId == null)
            return null;

        return _byId.TryGetValue(id, out var ab) ? ab : null;
    }
}
