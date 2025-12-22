using Features.Items.Data;

namespace Features.Items.Domain
{
    /// <summary>
    /// Runtime-экземпляр предмета.
    /// Никогда не должен быть null.
    /// Пустой предмет представлен через ItemInstance.Empty.
    /// </summary>
    public class ItemInstance
    {
        // =====================================================
        // EMPTY (Null Object Pattern)
        // =====================================================

        public static readonly ItemInstance Empty = new ItemInstance();

        // =====================================================
        // DATA
        // =====================================================

        public readonly Item itemDefinition; // ScriptableObject
        public int quantity;                 // runtime количество
        public int level;                    // runtime уровень

        // =====================================================
        // CONSTRUCTORS
        // =====================================================

        /// <summary>
        /// Приватный конструктор для Empty.
        /// </summary>
        private ItemInstance()
        {
            itemDefinition = null;
            quantity = 0;
            level = 0;
        }

        /// <summary>
        /// Обычный runtime-предмет.
        /// </summary>
        public ItemInstance(Item definition, int quantity = 1, int level = 0)
        {
            itemDefinition = definition;
            this.quantity = quantity;
            this.level = level;
        }

        // =====================================================
        // STATE
        // =====================================================

        public bool IsEmpty =>
            itemDefinition == null || quantity <= 0;

        public bool IsStackable =>
            !IsEmpty && itemDefinition.isStackable;

        public int MaxStack =>
            IsEmpty ? 0 : itemDefinition.maxStackAmount;

        // =====================================================
        // CLONE
        // =====================================================

        /// <summary>
        /// Полная копия предмета.
        /// </summary>
        public ItemInstance Clone()
        {
            if (IsEmpty)
                return Empty;

            return new ItemInstance(
                itemDefinition,
                quantity,
                level
            );
        }

        /// <summary>
        /// Копия предмета с новым количеством.
        /// </summary>
        public ItemInstance CloneWithQuantity(int newQuantity)
        {
            if (IsEmpty || newQuantity <= 0)
                return Empty;

            return new ItemInstance(
                itemDefinition,
                newQuantity,
                level
            );
        }
    }
}
