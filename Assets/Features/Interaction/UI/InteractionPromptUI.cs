using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Player;
using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private InteractionService interactionService;
    private InteractionRayService rayService;
    private INearbyInteractables nearby;

    private void Awake()
    {
        interactionService = new InteractionService();
    }

    private void Start()
    {
        rayService = InteractionServiceProvider.Ray;

        if (rayService == null)
        {
            Debug.LogError("[InteractionPromptUI] InteractionRayService NOT FOUND");
            enabled = false;
            return;
        }

        nearby = LocalPlayerContext.Get<NearbyInteractables>();

        promptText.text = "";
        promptText.enabled = false;
    }

    private void Update()
    {
        // 1) Nearby items
        if (nearby != null && Camera.main != null)
        {
            var best = nearby.GetBestItem(Camera.main);
            if (best != null && best.instance?.itemDefinition != null)
            {
                var def = best.instance.itemDefinition;
                int qty = best.instance.quantity;

                promptText.enabled = true;
                promptText.text = qty > 1
                    ? $"[E] Подобрать: {def.itemName} x{qty}"
                    : $"[E] Подобрать: {def.itemName}";
                return;
            }
        }

        // 2) Ray interactable
        var hit = rayService.Raycast();
        if (interactionService.TryGetInteractable(hit, out var interactable))
        {
            promptText.enabled = true;
            promptText.text = $"[E] {interactable.InteractionPrompt}";
            return;
        }

        promptText.enabled = false;
    }
}
