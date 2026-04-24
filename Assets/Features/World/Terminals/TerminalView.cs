using Features.Multiplayer.SceneBinding;
using UnityEngine;

namespace Features.World.Terminals
{
    public sealed class TerminalView : SceneBoundViewBase
    {
        [SerializeField] private GameObject poweredVisual;
        [SerializeField] private GameObject busyVisual;

        protected override string DefaultBoundType => "terminal";

        public void SetState(bool powered, bool busy)
        {
            if (poweredVisual != null)
                poweredVisual.SetActive(powered);

            if (busyVisual != null)
                busyVisual.SetActive(busy);
        }
    }
}
