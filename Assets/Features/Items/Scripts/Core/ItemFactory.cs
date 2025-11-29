using UnityEngine;

public static class ItemFactory
{
    public static Item CreateRuntimeItem(Item original)
    {
        if (original == null)
            return null;

        Item instance = Object.Instantiate(original);

        instance.currentLevel = 0;

        return instance;
    }
}
