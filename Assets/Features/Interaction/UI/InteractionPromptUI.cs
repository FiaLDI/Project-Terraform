using UnityEngine;
using TMPro;
using Features.Interaction.UnityIntegration;
using Features.Player.UI;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Items.UnityIntegration;

public sealed class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private InteractionResolver resolver;
    private INearbyInteractables nearby;

    private bool initialized;

    // ======================================================
    // UNITY
    // ======================================================

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
        // 🔥 СБРОС старых ссылок (обязательно)
        resolver = null;
        nearby = null;
        initialized = false;

        if (player == null)
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
        resolver = new InteractionResolver(ray, nearby);
        initialized = true;
        Debug.Log("[InteractionPromptUI] Resolver initialized");
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        // 🔒 полная защита
        if (!initialized ||
            resolver == null ||
            Camera.main == null ||
            nearby == null ||
            nearby is UnityEngine.Object o && o == null)
        {
            promptText.enabled = false;
            return;
        }

        var target = resolver.Resolve(Camera.main);

        switch (target.Type)
        {
            case InteractionTargetType.Pickup:
            {
                WorldItemNetwork worldItem = target.WorldItem;
                if (worldItem == null || !worldItem.IsPickupAvailable)
                    break;

                var inst = worldItem.GetComponent<ItemRuntimeHolder>()?.Instance;
                if (inst == null || inst.itemDefinition == null)
                    break;

                promptText.enabled = true;
                promptText.text =
                    inst.quantity > 1
                        ? $"[E] Подобрать: {inst.itemDefinition.itemName} x{inst.quantity}"
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
