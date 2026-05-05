using TMPro;
using UnityEngine;
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
    private bool subscribedToPlayerRoot;

    private void Awake()
    {
        if (promptText == null)
        {
            Debug.LogError("[InteractionPromptUI] promptText is NULL", this);
            enabled = false;
            return;
        }

        promptText.text = string.Empty;
        promptText.enabled = false;
    }

    private void OnEnable()
    {
        TrySubscribePlayerRoot();
    }

    private void OnDisable()
    {
        UnsubscribePlayerRoot();
        InteractionServiceProvider.OnRayInitialized -= InitResolver;
        ResetBindingState();
    }

    private void Update()
    {
        if (!subscribedToPlayerRoot)
            TrySubscribePlayerRoot();

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
                var worldItem = target.WorldItem;
                if (worldItem == null || !worldItem.IsPickupAvailable)
                    break;

                var inst = worldItem.GetComponent<ItemRuntimeHolder>()?.Instance;
                if (inst == null || inst.itemDefinition == null)
                    break;

                promptText.enabled = true;
                promptText.text =
                    inst.quantity > 1
                        ? $"[E] РџРѕРґРѕР±СЂР°С‚СЊ: {inst.itemDefinition.itemName} x{inst.quantity}"
                        : $"[E] РџРѕРґРѕР±СЂР°С‚СЊ: {inst.itemDefinition.itemName}";
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

    private void OnPlayerBound(GameObject player)
    {
        ResetBindingState();

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
            InteractionServiceProvider.OnRayInitialized -= InitResolver;
            InteractionServiceProvider.OnRayInitialized += InitResolver;
        }
    }

    private void InitResolver(InteractionRayService ray)
    {
        InteractionServiceProvider.OnRayInitialized -= InitResolver;

        if (nearby == null)
            return;

        resolver = new InteractionResolver(ray, nearby);
        initialized = true;
        Debug.Log("[InteractionPromptUI] Resolver initialized");
    }

    private void TrySubscribePlayerRoot()
    {
        var root = PlayerUIRoot.I;
        if (root == null)
            return;

        root.OnPlayerBound -= OnPlayerBound;
        root.OnPlayerBound += OnPlayerBound;
        subscribedToPlayerRoot = true;

        if (root.BoundPlayer != null)
            OnPlayerBound(root.BoundPlayer);
    }

    private void UnsubscribePlayerRoot()
    {
        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.OnPlayerBound -= OnPlayerBound;

        subscribedToPlayerRoot = false;
    }

    private void ResetBindingState()
    {
        resolver = null;
        nearby = null;
        initialized = false;

        if (promptText != null)
            promptText.enabled = false;
    }
}
