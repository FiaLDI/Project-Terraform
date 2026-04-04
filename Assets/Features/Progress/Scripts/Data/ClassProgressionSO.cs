
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progression/Class Progression")]
public class ClassProgressionSO : ScriptableObject
{
    public List<ProgressionNodeSO> nodes;
}