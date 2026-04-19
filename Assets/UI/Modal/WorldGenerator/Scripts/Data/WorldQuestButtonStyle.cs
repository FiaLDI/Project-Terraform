using UnityEngine;

[CreateAssetMenu(fileName = "WorldQuestButtonStyle", menuName = "Game/UI/World Quest Button Style")]
public sealed class WorldQuestButtonStyle : ScriptableObject
{
    [Header("Colors")]
    public Color normalBackgroundColor = new Color(0.15f, 0.19f, 0.22f, 0.96f);
    public Color selectedBackgroundColor = new Color(0.27f, 0.43f, 0.48f, 0.98f);
    public Color labelColor = new Color(0.97f, 0.99f, 1f, 0.98f);
}
