using Features.Items.Domain;
using Features.Tools.Domain;

namespace Features.Tools.Application
{
    public class ToolService
    {
        public ToolRuntimeStats stats;

        public void Initialize(ItemInstance instance)
        {
            stats = ToolStatCalculator.Calculate(instance);
        }

        public void ApplyUseEffects(ItemInstance instance)
        {
            // Уменьшаем прочность
            instance.quantity--; // пример, у тебя может быть durability отдельно

            // TODO: можно расширить
        }
    }
}
