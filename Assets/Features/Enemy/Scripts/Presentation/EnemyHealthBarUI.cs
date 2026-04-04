using Features.Camera.UnityIntegration;
using Features.Stats.Adapter;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Enemy
{
    [RequireComponent(typeof(Canvas))]
    public sealed class EnemyHealthBarUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;

        [Header("Target")]
        [SerializeField] private HealthStatsAdapter target;
        [SerializeField] private Transform headAnchor;

        private Canvas canvas;
        private UnityEngine.Camera cam;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }

        private void OnEnable()
        {
            // Камера
            if (CameraRegistry.Instance != null)
            {
                CameraRegistry.Instance.OnCameraChanged += HandleCameraChanged;

                if (CameraRegistry.Instance.CurrentCamera != null)
                    HandleCameraChanged(CameraRegistry.Instance.CurrentCamera);
            }

            // Target
            if (target == null)
                target = GetComponentInParent<HealthStatsAdapter>();

            if (target != null)
                target.OnHealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (CameraRegistry.Instance != null)
                CameraRegistry.Instance.OnCameraChanged -= HandleCameraChanged;

            if (target != null)
                target.OnHealthChanged -= OnHealthChanged;
        }

        private void Start()
        {
            if (fillImage == null)
            {
                Debug.LogError("[EnemyHealthBarUI] FillImage missing", this);
                enabled = false;
                return;
            }

            if (target == null)
            {
                Debug.LogError("[EnemyHealthBarUI] HealthStatsAdapter not found", this);
                enabled = false;
                return;
            }

            if (target.IsReady)
                OnHealthChanged(target.CurrentHp, target.MaxHp);
        }

        private void LateUpdate()
        {
            if (cam == null || headAnchor == null)
                return;

            transform.position = headAnchor.position;
            transform.LookAt(cam.transform);
        }

        // =====================================================
        // EVENT
        // =====================================================

        private void OnHealthChanged(float hp, float maxHp)
        {
            float fill = maxHp > 0f ? hp / maxHp : 0f;
            fillImage.fillAmount = fill;

            if (hp <= 0f)
                Invoke(nameof(Hide), 1f);
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleCameraChanged(UnityEngine.Camera newCam)
        {
            cam = newCam;
            canvas.worldCamera = cam;
        }
    }
}