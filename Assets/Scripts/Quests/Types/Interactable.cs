using UnityEngine;
using UnityEngine.InputSystem;
using Quests;

public class Interactable : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public string interactText = "Взаимодействовать";
    public float interactionDistance = 3f;

    [Header("Связанный квест")]
    public Quest linkedQuest;

    [Header("Визуальные эффекты")]
    public GameObject highlightEffect;
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;

    [Header("Input System")]
    public PlayerInput playerInput;
    private InputAction interactAction;

    private bool isPlayerNear = false;
    private Transform player;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (highlightEffect != null)
            highlightEffect.SetActive(false);

        if (player == null)
        {
            Debug.LogError("Interactable: Не найден объект с тегом 'Player'");
        }

        InitializeInputSystem();
    }

    void InitializeInputSystem()
    {
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("Interactable: PlayerInput не найден в сцене!");
                return;
            }
        }

        interactAction = playerInput.actions.FindAction("Interact");
        if (interactAction != null)
        {
            interactAction.performed += OnInteractPerformed;
            Debug.Log("Interactable: Input Action 'Interact' привязан");
        }
        else
        {
            Debug.LogError("Interactable: Action 'Interact' не найден в Input Action Asset!");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerNear = distance <= interactionDistance;

        UpdateHighlight();
    }

    void UpdateHighlight()
    {
        if (objectRenderer != null && highlightMaterial != null)
        {
            objectRenderer.material = isPlayerNear ? highlightMaterial : originalMaterial;
        }

        if (highlightEffect != null)
        {
            highlightEffect.SetActive(isPlayerNear);
        }
    }
    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isPlayerNear)
        {
            OnInteract();
        }
    }

    void OnInteract()
    {
        Debug.Log($"Взаимодействие с: {gameObject.name}");

        if (linkedQuest != null && linkedQuest.isActive)
        {
            if (Quests.QuestManager.Instance != null)
            {
                Quests.QuestManager.Instance.UpdateQuestProgress(linkedQuest, 1);
                Debug.Log($"Прогресс квеста '{linkedQuest.questName}' обновлен");
            }
            else
            {
                Debug.LogError("QuestManager.Instance не найден!");
            }
        }

        OnInteractionComplete();
    }

    protected virtual void OnInteractionComplete()
    {
    }

    void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }

    void OnEnable()
    {
        if (interactAction != null)
            interactAction.Enable();
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.Disable();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}