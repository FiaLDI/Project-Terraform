
namespace Features.Stats.Domain
{   
    public interface IStatsOwner
    {
        IStatsFacade Facade { get; }
        bool IsReady { get; }
    }
}
