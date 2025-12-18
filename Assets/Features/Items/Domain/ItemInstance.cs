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

        // =====================================================
        // CLONE
        // =====================================================

        /// <summary>
        /// Полная копия предмета (со всем количеством).
        /// </summary>
        public ItemInstance Clone()
        {
            return new ItemInstance(
                itemDefinition,
                quantity,
                level
            );
        }

        /// <summary>
        /// Копия предмета с указанным количеством.
        /// Используется для split stack / drop 1.
        /// </summary>
        public ItemInstance CloneWithQuantity(int newQuantity)
        {
            return new ItemInstance(
                itemDefinition,
                newQuantity,
                level
            );
        }
    }
}
