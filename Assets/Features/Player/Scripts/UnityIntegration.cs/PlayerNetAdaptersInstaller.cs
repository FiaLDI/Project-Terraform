using UnityEngine;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Гарантирует наличие Net-адаптеров на Player.
    /// Никакой логики — только установка компонентов.
    /// </summary>
    public sealed class PlayerNetAdaptersInstaller : MonoBehaviour
    {
        private void Awake()
        {
            Ensure<PlayerMovementNetAdapter>();
            Ensure<AbilityCasterNetAdapter>();
        }

        private void Ensure<T>() where T : Component
        {
            if (!TryGetComponent<T>(out _))
            {
                gameObject.AddComponent<T>();
#if UNITY_EDITOR
                Debug.Log($"[PlayerNetAdaptersInstaller] Added {typeof(T).Name} to {name}");
#endif
            }
        }
    }
}
