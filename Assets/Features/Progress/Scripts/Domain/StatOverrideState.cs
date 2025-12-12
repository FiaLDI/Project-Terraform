using System;
using System.Collections.Generic;

[Serializable]
public class StatOverrideState
{
    public Dictionary<string, float> flat = new Dictionary<string, float>();
    public Dictionary<string, float> percent = new Dictionary<string, float>();
}
