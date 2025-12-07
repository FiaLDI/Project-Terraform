using UnityEngine;
using UnityEngine.UI;
using Features.Interaction.Application;
using Features.Interaction.UnityIntegration;

namespace Features.Player.UI
{
    public class CrosshairController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image crosshair;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color interactColor = Color.cyan;

        private InteractionService interactionService = new InteractionService();

        private void Update()
        {
            if (crosshair == null) return;

            var hit = InteractionServiceProvider.Ray.Raycast();

            if (interactionService.TryGetInteractable(hit, out _))
                crosshair.color = interactColor;
            else
                crosshair.color = normalColor;
        }
    }
}
