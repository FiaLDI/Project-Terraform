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

            var service = InteractionServiceProvider.Ray;
            if (service == null)
            {
                crosshair.color = normalColor;
                return;
            }

            var rayProvider = service.Provider;
            if (!rayProvider.IsValid())
            {
                crosshair.color = normalColor;
                return;
            }

            var hit = service.Raycast();

            if (interactionService.TryGetInteractable(hit, out _))
                crosshair.color = interactColor;
            else
                crosshair.color = normalColor;
        }


    }
}
