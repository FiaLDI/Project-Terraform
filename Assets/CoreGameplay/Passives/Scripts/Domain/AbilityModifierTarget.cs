using UnityEngine;

[System.Serializable]
public struct AbilityModifierTarget
{
    public string abilityId;
    public AbilityTag tags;

    public bool Matches(string id, AbilityTag abilityTags)
    {
        if (!string.IsNullOrEmpty(abilityId) && abilityId != id)
            return false;

        if (tags != AbilityTag.None && (abilityTags & tags) == 0)
            return false;

        return true;
    }
}
