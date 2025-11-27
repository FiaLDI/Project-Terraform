public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;

    public void Init(UpgradeStation station, CraftingProcessor processor)
    {
        this.station = station;
        base.Init(processor, station.GetRecipes());
    }
}
