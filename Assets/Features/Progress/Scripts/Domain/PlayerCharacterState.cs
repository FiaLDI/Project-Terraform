using System;
using System.Collections.Generic;

[Serializable]
public class PlayerCharacterState
{
    public string characterId;
    public string classId;

    public string nickname;

    public int level = 1;
    public int experience = 0;

    public string specializationId = null;

    public List<string> abilities = new List<string>();
    public List<string> passives = new List<string>();

    public List<string> equippedModules = new List<string>();
    public List<string> unlockedModules = new List<string>();

    public CharacterInventoryState characterInventory = new CharacterInventoryState();

    public string visualPresetId = "default";
    public List<string> cosmeticItems = new List<string>();

    public StatOverrideState statsOverrides = new StatOverrideState();

    public long createdAt;
    public long lastUsedAt;

    public PlayerCharacterState()
    {
        createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        lastUsedAt = createdAt;
    }
}
