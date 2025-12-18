using Features.Items.Data;

namespace Features.Items.Application
{
    public interface IItemStatCalculator
    {
        ItemRuntimeStats Calculate(Item item, int level);
    }
}
