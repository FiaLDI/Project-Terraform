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

    private bool initialized;

    private void Awake()
    {
        interactionService = new InteractionService();
        promptText.text = "";
        promptText.enabled = false;
    }

    private void Start()
    {
        rayService = InteractionServiceProvider.Ray;

        if (rayService == null)
        {
            enabled = false;
            return;
        }

        if (LocalPlayerContext.IsReady)
            Init();
        else
            LocalPlayerContext.OnReady += Init;
    }

    private void OnDestroy()
    {
        LocalPlayerContext.OnReady -= Init;
    }

    private void Init()
    {
        if (initialized)
            return;

        nearby = LocalPlayerContext.Get<NearbyInteractables>();

        if (nearby == null)
        {
            Debug.LogWarning(
                "[InteractionPromptUI] NearbyInteractables not found on LocalPlayer"
            );
            return;
        }

        initialized = true;
    }

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
