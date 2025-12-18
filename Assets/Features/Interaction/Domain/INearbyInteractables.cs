using UnityEngine;

namespace Features.Interaction.Domain
{
    public interface INearbyInteractables
    {
        NearbyItemPresenter GetBestItem(UnityEngine.Camera cam);
        void Register(NearbyItemPresenter presenter);
        void Unregister(NearbyItemPresenter presenter);
    }
}
