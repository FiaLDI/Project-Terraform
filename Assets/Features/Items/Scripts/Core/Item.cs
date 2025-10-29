using UnityEngine;

public enum ItemType { Resource, Tool, Weapon, Ammo, Quest }
public abstract class Item : ScriptableObject
{

    [Header("In-Game Model")]
    public GameObject worldPrefab;

    [Header("Item Info")]
    public int id;
    public string itemName;
    [TextArea(4, 4)]
    public string description;
    public Sprite icon;

    [Header("Stacking")]
    public bool isStackable;
    public int maxStackAmount = 1; // только если isStackable = true
    [Header("General")]
    public ItemType itemType;

    [Header("Quest Integration")]
    public bool isQuestItem = false;
    public string questId = ""; // можно оставить пустым, если не св€зано
    public int requiredAmount = 1; // если предмет участвует в сборе

    // ѕри создании нового предмета в редакторе, убедимс€, что
    // если он не стакаетс€, то максимальный размер стака равен 1.
    private void OnValidate()
    {
        if (!isStackable)
        {
            maxStackAmount = 1;
        }
    }
}