using UnityEngine;

public class MaterialProcessorUIController : BaseStationUI
{
    private MaterialProcessor station;

    public void Init(MaterialProcessor station, CraftingProcessor processor)
    {
        this.station = station;

        base.Init(processor, station.GetRecipes());
    }
}
