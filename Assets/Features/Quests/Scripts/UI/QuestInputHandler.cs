using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests
{
    public class QuestInputHandler : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private QuestUI questUI;

        private InputSystem_Actions inputActions;

        private void Awake()
        {
            Debug.Log("QuestInputHandler: подписка на ToggleQuests создана");

            inputActions = new InputSystem_Actions();

            // Подписка на действие ToggleQuests (например, клавиша J)
            inputActions.UI.ToggleQuests.performed += ctx => ToggleQuests();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void ToggleQuests()
        {
            if (questUI != null)
            {
                questUI.ToggleAllQuestsPanel();
                Debug.Log("📖 QuestInputHandler: ToggleQuests вызван");
            }
        }
    }
}
