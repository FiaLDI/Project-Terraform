
using Features.Passives.Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progression/Node")]
public class ProgressionNodeSO : ScriptableObject
{
    public string id;

    [Header("Presentation")]
    public string displayName;

    [TextArea(2, 4)]
    public string description;

    [Header("Unlock")]
    public int requiredLevel;

    [Header("Reward")]
    public PassiveSO passive;

    [Header("UI")]
    public Vector2 position;
    public float uiSize = 72f;
}
