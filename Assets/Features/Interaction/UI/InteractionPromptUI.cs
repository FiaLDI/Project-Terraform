using UnityEngine;
using TMPro;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Player.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private InteractionService interactionService;
    private InteractionRayService rayService;
    private INearbyInteractables nearby;

    private bool initialized;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    private void Awake()
    {
        interactionService = new InteractionService();

        promptText.text = "";
        promptText.enabled = false;

        Debug.Log("[InteractionPromptUI] Awake", this);
    }

    private void OnEnable()
    {
        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.OnPlayerBound += OnPlayerBound;
    }

    private void OnDisable()
    {
        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.OnPlayerBound -= OnPlayerBound;
    }

    // ======================================================
    // PLAYER BIND
    // ======================================================

    private void OnPlayerBound(GameObject player)
    {
        if (initialized)
            return;

        Debug.Log("[InteractionPromptUI] OnPlayerBound: " + player.name, this);

        nearby = player.GetComponentInChildren<INearbyInteractables>();
        if (nearby == null)
        {
            Debug.LogWarning(
                "[InteractionPromptUI] NearbyInteractables not found on player",
                player
            );
            return;
        }

        rayService = InteractionServiceProvider.Ray;
        if (rayService == null)
        {
            Debug.LogError(
                "[InteractionPromptUI] InteractionRayService not ready"
            );
            return;
        }

        initialized = true;
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        if (!initialized || Camera.main == null)
            return;

        // ==== PICKUPS ====
        var best = nearby.GetBestItem(Camera.main);
        if (best != null && best.GetInstance()?.itemDefinition != null)
        {
            var def = best.GetInstance().itemDefinition;
            int qty = best.GetInstance().quantity;

            promptText.enabled = true;
            promptText.text = qty > 1
                ? $"[E] Подобрать: {def.itemName} x{qty}"
                : $"[E] Подобрать: {def.itemName}";
            return;
        }

        // ==== INTERACTABLES ====
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
