using UnityEngine;
using Features.Input;
using Features.UI;

namespace Features.World.UI
{
    public sealed class WorldGeneratorUI : PlayerBoundUIView, IUIScreen
    {
        [SerializeField] private GameObject root;

        public InputMode Mode => InputMode.Dialog;

        protected override void OnEnable()
        {
            base.OnEnable();
            UIRegistry.I?.Register(this);
            root.SetActive(false);
        }

        protected override void OnDisable()
        {
            UIRegistry.I?.Unregister(this);
            base.OnDisable();
        }

        protected override void OnPlayerBound(GameObject player)
        {
            root.SetActive(false);
        }

        public void Show()
        {
            root.SetActive(true);
            InputModeManager.I.SetMode(Mode);
        }

        public void Hide()
        {
            root.SetActive(false);
            InputModeManager.I.SetMode(InputMode.Gameplay);
        }

        public void Open()
        {
            UIStackManager.I.Push(this);
        }

        public void OnGenerateWorldClicked()
        {
            UIStackManager.I?.Clear();
            SceneTransitionService.RequestWorldScene();
        }

        public void OnCloseClicked()
        {
            UIStackManager.I.Pop();
        }

    }
}
