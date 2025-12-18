using UnityEngine;

namespace Features.Player
{
    public class PlayerInputContext : MonoBehaviour
    {
        private InputSystem_Actions _actions;

        public InputSystem_Actions Actions
        {
            get
            {
                if (_actions == null)
                    Init();
                return _actions;
            }
        }

        private void Init()
        {
            _actions = new InputSystem_Actions();

            // UI всегда активно
            _actions.UI.Enable();

            // Player по умолчанию
            _actions.Player.Enable();
            _actions.Inventory.Disable();
        }

        private void OnDisable()
        {
            _actions?.Disable();
        }
    }
}
