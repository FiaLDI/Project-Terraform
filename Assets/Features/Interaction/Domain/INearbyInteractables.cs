using UnityEngine;
using Features.Items.UnityIntegration;

namespace Features.Interaction.Domain
{
    public interface INearbyInteractables
    {
        WorldItemNetwork GetBestItem(UnityEngine.Camera cam);
        void Register(WorldItemNetwork item);
        void Unregister(WorldItemNetwork item);
    }
}
