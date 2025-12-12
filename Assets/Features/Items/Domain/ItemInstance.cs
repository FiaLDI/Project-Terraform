using Features.Items.Data;

namespace Features.Items.Domain
{
    /// <summary>
    /// Runtime-экземпляр предмета.
    /// Хранит состояние, которое нельзя держать в ScriptableObject.
    /// </summary>
    public class ItemInstance
    {
        public readonly Item itemDefinition; // чистые данные из SO

        public int quantity;                 // runtime количество
        public int level;                    // runtime уровень (апгрейды)

        public ItemInstance(Item definition, int quantity = 1, int level = 0)
        {
            itemDefinition = definition;
            this.quantity = quantity;
            this.level = level;
        }

        public bool IsStackable =>
            itemDefinition.isStackable;

        public int MaxStack =>
            itemDefinition.maxStackAmount;
    }
}
