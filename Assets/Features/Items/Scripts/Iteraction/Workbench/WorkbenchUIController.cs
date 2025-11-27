public class WorkbenchUIController : BaseStationUI
{
    private Workbench station;

    public void Init(Workbench station, CraftingProcessor processor)
    {
        this.station = station;
        base.Init(processor, station.GetRecipes());
    }
}
