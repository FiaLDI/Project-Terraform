using UnityEngine;

[CreateAssetMenu(fileName = "WorldRegionButtonStyle", menuName = "Game/UI/World Region Button Style")]
public sealed class WorldRegionButtonStyle : ScriptableObject
{
    [Header("Animation")]
    public float hoverHighlight = 1.15f;
    public float selectedHighlight = 0.6f;
    public float fadeSpeed = 8f;

    [Header("Fallback Colors")]
    public Color defaultLockedColor = new Color(0.08f, 0.18f, 0.18f, 0.18f);

    [Header("Label")]
    public Color labelColor = new Color(0.96f, 0.98f, 0.99f, 0.96f);
}
