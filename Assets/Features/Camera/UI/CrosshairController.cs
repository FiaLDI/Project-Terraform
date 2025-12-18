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

        private void OnEnable()
        {
            if (InteractionServiceProvider.Ray != null)
            {
                OnRayReady(InteractionServiceProvider.Ray);
            }
            else
            {
                InteractionServiceProvider.OnRayInitialized += OnRayReady;
            }
        }

        private void OnDisable()
        {
            InteractionServiceProvider.OnRayInitialized -= OnRayReady;
        }

        private void OnRayReady(InteractionRayService ray)
        {
            rayService = ray;
            Debug.Log("[CrosshairController] InteractionRayService READY");
        }

        private void Update()
        {
            if (crosshair == null || rayService == null)
                return;

            InteractionRayHit hit = rayService.Raycast();

            crosshair.color =
                interactionService.TryGetInteractable(hit, out _)
                    ? interactColor
                    : normalColor;
        }
    }
}
