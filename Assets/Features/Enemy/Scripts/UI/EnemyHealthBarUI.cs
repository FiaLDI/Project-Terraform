using UnityEngine;
using UnityEngine.UI;
using Features.Camera.UnityIntegration;
using Features.Enemy.UnityIntegration;
using Features.Stats.Domain;


namespace Features.Enemy
{
    [RequireComponent(typeof(Canvas))]
    public sealed class EnemyHealthBarUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;

        [Header("Target")]
        [SerializeField] private IStatsFacade target;
        [SerializeField] private Transform headAnchor;

        private Canvas canvas;
        private UnityEngine.Camera cam;

        private float current;
        private float max;
        private float targetFill = 1f;

        private float updateTimer;

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
            if (CameraRegistry.Instance != null)
            {
                CameraRegistry.Instance.OnCameraChanged += HandleCameraChanged;

                if (CameraRegistry.Instance.CurrentCamera != null)
                    HandleCameraChanged(CameraRegistry.Instance.CurrentCamera);
            }
        }

        private void Start()
        {
            if (target == null)
                target = GetComponentInParent<IStatsFacade>();

            if (target == null)
            {
                Debug.LogError("[EnemyHealthBarUI] EnemyHealth not found", this);
                enabled = false;
                return;
            }

            if (fillImage == null)
            {
                Debug.LogError("[EnemyHealthBarUI] FillImage missing", this);
                enabled = false;
                return;
            }

            UpdateFillImmediate();
        }

        private void LateUpdate()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < 0.05f)
                return;

            updateTimer = 0f;

            if (cam == null || headAnchor == null)
                return;

            transform.position = headAnchor.position;
            transform.LookAt(cam.transform, Vector3.up);

            targetFill = target.Health.CurrentHp;

            fillImage.fillAmount = targetFill;
        }

        private void OnDisable()
        {
            if (CameraRegistry.Instance != null)
                CameraRegistry.Instance.OnCameraChanged -= HandleCameraChanged;
        }

        // =====================================================
        // VIEW API (вызывается EnemyHealth)
        // =====================================================

        public void SetHealth(float hp, float maxHp)
        {
            current = hp;
            max = maxHp;

            targetFill = max > 0f ? current / max : 0f;

            if (current <= 0f)
                Invoke(nameof(Hide), 1f);
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private void UpdateFillImmediate()
        {
            targetFill = max > 0f ? current / max : 1f;
            fillImage.fillAmount = targetFill;
        }

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
