# Features Crafting

This subsystem handles recipes, crafting stations, processor timing, and the UI that drives those flows.

## Main pieces

- `RecipeSO`, `ProcessingRecipeSO`, and `UpgradeRecipeSO` define authoring assets for craftable content.
- `RecipeDatabase` loads the shared recipe list and caches recipes by station type and recipe id.
- `CraftingProcessor`, `MaterialProcessor`, and `UpgradeProcessor` manage local processing flows and progress events.
- Station and processor UI lives under `Stations` and `UI`.

## Runtime flow

1. A station UI selects a recipe from `RecipeDatabase`.
2. A processor starts local progress and emits `OnStart`, `OnProgress`, and `OnComplete`.
3. When processing completes, the subsystem does not directly mutate inventory.
4. Instead it sends an inventory command such as `InventoryCommand.CraftRecipe` through the player network path.
5. Inventory and recipe resolution on the server produce the final item changes.

## Integration points

- `Inventory` supplies ingredients and receives the crafted result.
- `Items` supplies the item definitions referenced by recipes.
- `Multiplayer` is used for authoritative inventory command execution.
