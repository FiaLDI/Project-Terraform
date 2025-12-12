using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Player.UI
{
    public class CrosshairController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image crosshair;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color interactColor = Color.cyan;

        private InteractionService interactionService;
        private InteractionRayService rayService;

        private void Awake()
        {
            interactionService = new InteractionService();
        }

        private void Start()
        {
            rayService = InteractionServiceProvider.Ray;
            if (rayService == null)
            {
                Debug.LogError("[CrosshairController] InteractionRayService NOT FOUND");
                enabled = false;
            }
        }

        private void Update()
        {
            if (crosshair == null || rayService == null)
                return;

            InteractionRayHit hit = rayService.Raycast();

            if (interactionService.TryGetInteractable(hit, out _))
                crosshair.color = interactColor;
            else
                crosshair.color = normalColor;
        }
    }
}
