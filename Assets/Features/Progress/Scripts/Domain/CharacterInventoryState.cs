using System;
using System.Collections.Generic;

[Serializable]
public class CharacterInventoryState
{
    public Dictionary<string, int> items = new Dictionary<string, int>();
    public Dictionary<string, int> resources = new Dictionary<string, int>();
    public Dictionary<string, int> modules = new Dictionary<string, int>();

    public Dictionary<string, int> missionLoot = new Dictionary<string, int>();

    public int capacity = 100;
}
