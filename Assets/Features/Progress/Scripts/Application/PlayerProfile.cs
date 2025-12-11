using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProfile
{
    public List<PlayerCharacterState> characters = new List<PlayerCharacterState>();
    public int activeCharacterIndex = -1;

    public GlobalStorageState globalStorage = new GlobalStorageState();
    public ResearchState techTree = new ResearchState();
    public SquadRankState squadRank = new SquadRankState();

    public PlayerCharacterState GetActiveCharacter()
    {
        if (activeCharacterIndex < 0 || activeCharacterIndex >= characters.Count)
            return null;

        return characters[activeCharacterIndex];
    }
}
