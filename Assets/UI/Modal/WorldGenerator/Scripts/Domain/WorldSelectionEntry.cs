using UnityEngine;
using Features.Quests.Data;
using Biomes.Data;

[System.Serializable]
public sealed class WorldSelectionEntry
{
    [Header("World")]
    public WorldConfig worldConfig;

    [Header("Texts")]
    public string displayName;

    [TextArea(2, 5)]
    public string description;

    [Header("Map Layout")]
    public Vector2 position;
    public Vector2 size = new Vector2(190f, 200f);
    public float rotation;

    [Header("Visuals")]
    public Sprite regionSprite;
    public Color idleColor = new Color(0.25f, 0.55f, 0.58f, 0.24f);
    public Color selectedColor = new Color(0.45f, 0.8f, 0.84f, 0.38f);
    public Color lockedColor = new Color(0f, 0f, 0f, 0f);

    [Header("Content")]
    public QuestAsset[] availableQuests;
    public QuestChainAsset[] availableChains;
}
