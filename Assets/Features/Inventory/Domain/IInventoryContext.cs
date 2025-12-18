using Features.Inventory.Domain;
using Features.Inventory.Application;

namespace Features.Inventory
{
    public interface IInventoryContext
    {
        InventoryModel Model { get; }
        InventoryService Service { get; }
    }
}
