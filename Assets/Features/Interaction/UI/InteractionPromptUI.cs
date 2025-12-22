using UnityEngine;
using TMPro;
using Features.Interaction.UnityIntegration;
using Features.Player.UI;
using Features.Interaction.Application;
using Features.Interaction.Domain;

public sealed class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private InteractionResolver resolver;
    private INearbyInteractables nearby;

    private bool initialized;

    private void Awake()
    {
        if (promptText == null)
        {
            Debug.LogError("[InteractionPromptUI] promptText is NULL", this);
            enabled = false;
            return;
        }

        promptText.text = "";
        promptText.enabled = false;
    }

    private void Start()
    {
        // ✅ гарантированно после всех Awake
        if (PlayerUIRoot.I == null)
        {
            Debug.LogError("[InteractionPromptUI] PlayerUIRoot.I is NULL in Start");
            return;
        }

        PlayerUIRoot.I.OnPlayerBound += OnPlayerBound;
    }

    private void OnDestroy()
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

        Debug.Log("[InteractionPromptUI] Player bound: " + player.name);

        nearby = player.GetComponentInChildren<INearbyInteractables>();
        if (nearby == null)
        {
            Debug.LogError("[InteractionPromptUI] NearbyInteractables NOT FOUND", player);
            return;
        }

        if (InteractionServiceProvider.Ray != null)
        {
            InitResolver(InteractionServiceProvider.Ray);
        }
        else
        {
            InteractionServiceProvider.OnRayInitialized += InitResolver;
        }
    }

    private void InitResolver(InteractionRayService ray)
    {
        Debug.Log("[InteractionPromptUI] Resolver initialized");

        resolver = new InteractionResolver(ray, nearby);
        initialized = true;
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        if (!initialized || resolver == null || Camera.main == null)
        {
            promptText.enabled = false;
            return;
        }

        var target = resolver.Resolve(Camera.main);

        switch (target.Type)
        {
            case InteractionTargetType.Pickup:
            {
                var inst = target.Pickup?.GetInstance();
                if (inst?.itemDefinition == null)
                    break;

                int qty = inst.quantity;
                promptText.enabled = true;
                promptText.text = qty > 1
                    ? $"[E] Подобрать: {inst.itemDefinition.itemName} x{qty}"
                    : $"[E] Подобрать: {inst.itemDefinition.itemName}";
                return;
            }

            case InteractionTargetType.Interactable:
            {
                promptText.enabled = true;
                promptText.text = $"[E] {target.Interactable.InteractionPrompt}";
                return;
            }
        }

        promptText.enabled = false;
    }
}
