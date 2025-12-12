using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Orbis/Characters/Robot Visual Library")]
public class RobotVisualLibrarySO : ScriptableObject
{
    public List<RobotVisualPresetSO> presets;

    public RobotVisualPresetSO Find(string id)
    {
        return presets.Find(p => p.id == id);
    }
}
