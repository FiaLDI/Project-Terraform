using System;
using System.Collections.Generic;

[Serializable]
public class GlobalStorageState
{
    public Dictionary<string, int> resources = new Dictionary<string, int>();
    public Dictionary<string, int> materials = new Dictionary<string, int>();
    public Dictionary<string, int> modules = new Dictionary<string, int>();
    public Dictionary<string, int> items = new Dictionary<string, int>();

    public List<string> blueprints = new List<string>();

    public int capacity = 10000;
}
