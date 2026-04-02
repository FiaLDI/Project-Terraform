
using Features.Passives.Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progression/Node")]
public class ProgressionNodeSO : ScriptableObject
{
    public string id;

    [Header("Unlock")]
    public int requiredLevel;

    [Header("Reward")]
    public PassiveSO passive;

    [Header("UI")]
    public Vector2 position;
}
