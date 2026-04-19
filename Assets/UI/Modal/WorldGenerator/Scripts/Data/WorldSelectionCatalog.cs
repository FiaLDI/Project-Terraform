using UnityEngine;

[CreateAssetMenu(fileName = "WorldSelectionCatalog", menuName = "Game/World Selection Catalog")]
public sealed class WorldSelectionCatalog : ScriptableObject
{
    public WorldSelectionEntry[] entries;
}
