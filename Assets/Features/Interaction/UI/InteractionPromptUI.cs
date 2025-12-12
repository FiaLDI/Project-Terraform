using UnityEngine;
using TMPro;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;

    private InteractionRayService rayService;
    private InteractionService interactionService;

    private void Start()
    {
        var provider = FindObjectOfType<CameraRayProvider>();

        rayService = new InteractionRayService(
            provider,
            LayerMask.GetMask("Default", "Interactable", "Item"),
            LayerMask.GetMask("Player")
        );

        interactionService = new InteractionService();

        promptText.text = "";
        promptText.enabled = false;
    }

    private void Update()
    {
        // --- FIRST: nearest item via NearbyInteractables ---
        var cam = providerCam;
        var bestItem = NearbyInteractables.instance.GetBestItem(cam);

        if (bestItem != null)
        {
            promptText.enabled = true;
            promptText.text = $"[E] Подобрать: {bestItem.itemData.itemName}";
            return;
        }

        // --- SECOND: IInteractable via InteractionRayService ---
        var hit = rayService.Raycast();

        if (interactionService.TryGetInteractable(hit, out var interactable))
        {
            promptText.enabled = true;
            promptText.text = $"[E] {interactable.InteractionPrompt}";
            return;
        }

        // --- NOTHING FOUND ---
        promptText.enabled = false;
    }

    private Camera providerCam => FindObjectOfType<CameraRayProvider>().GetComponentInChildren<Camera>();
}
