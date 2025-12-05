using System.Collections.Generic;
using Features.Classes.Data;
    
namespace Features.Classes.Domain
{
    public interface IPlayerClassService
    {
        PlayerClassConfigSO Current { get; }
        void SelectClass(string id);
        void SelectClass(PlayerClassConfigSO config);
        PlayerClassConfigSO FindById(string id);
        IEnumerable<PlayerClassConfigSO> AllClasses { get; }
    }
}
