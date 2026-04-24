using Features.Multiplayer.SceneBinding;
using UnityEngine;

namespace Features.Interaction.UnityIntegration
{
    public sealed class SceneBoundInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private SceneBoundViewBase boundView;
        [SerializeField] private SceneBoundInteractionCommand command = SceneBoundInteractionCommand.Primary;
        [SerializeField] private string interactionPrompt = "Use";

        public string InteractionPrompt => interactionPrompt;

        private void Awake()
        {
            if (boundView == null)
                boundView = GetComponentInParent<SceneBoundViewBase>();
        }

        public bool Interact()
        {
            if (boundView == null)
                return false;

            if (!SceneBoundRegistry.TryGetController(boundView.BoundKey, out var controller))
            {
                Debug.LogWarning(
                    $"[SceneBoundInteractable] Controller not found for key={boundView.BoundKey}",
                    this
                );
                return false;
            }

            Debug.Log(
                $"[SceneBoundInteractable] Requesting command={command} key={boundView.BoundKey} controller={controller.name}",
                this
            );
            controller.RequestInteraction(command);
            return true;
        }
    }
}
