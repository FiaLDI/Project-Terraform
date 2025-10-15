using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests
{
    public class QuestInputHandler : MonoBehaviour
    {
        [Header("—сылки")]
        public QuestUI questUI;

        private PlayerInput playerInput;
        private InputAction toggleQuestsAction;

        private void Awake()
        {
            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                Debug.LogError("QuestInputHandler: PlayerInput не найден!");
                return;
            }

            toggleQuestsAction = playerInput.actions.FindAction("ToggleQuests");

            if (toggleQuestsAction != null)
            {
                toggleQuestsAction.performed += OnToggleQuests;
                Debug.Log("QuestInputHandler: Action ToggleQuests найден и прив€зан");
            }
            else
            {
                Debug.LogError("QuestInputHandler: Action 'ToggleQuests' не найден в Input Action Asset!");
            }
        }

        private void OnToggleQuests(InputAction.CallbackContext context)
        {
            if (questUI != null && context.performed)
            {
                questUI.ToggleAllQuestsPanel();
            }
        }

        private void OnEnable()
        {
            if (toggleQuestsAction != null)
                toggleQuestsAction.Enable();
        }

        private void OnDisable()
        {
            if (toggleQuestsAction != null)
                toggleQuestsAction.Disable();
        }

        private void OnDestroy()
        {
            if (toggleQuestsAction != null)
            {
                toggleQuestsAction.performed -= OnToggleQuests;
            }
        }
    }
}