using UnityEngine;
using UnityEngine.UI;
using Features.Camera.UnityIntegration;

namespace Features.Enemy
{
    [RequireComponent(typeof(Canvas))]
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;

        [Header("Target")]
        [SerializeField] private EnemyHealth target;
        [SerializeField] private Transform headAnchor;

        private Canvas canvas;
        private UnityEngine.Camera cam;

        private float targetFill = 1f;
        private float uiTimer;

        public EnemyHealth Target
        {
            get => target;
            set => target = value;
        }

        public Transform HeadAnchor
        {
            get => headAnchor;
            set => headAnchor = value;
        }

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }

        private void OnEnable()
        {
            // ������� ������� CameraRegistry
            if (CameraRegistry.Instance != null)
            {
                CameraRegistry.Instance.OnCameraChanged += HandleCameraChanged;

                // ���� ������ ��� ���� ���������������� � ������������� � �����
                if (CameraRegistry.Instance.CurrentCamera != null)
                {
                    HandleCameraChanged(CameraRegistry.Instance.CurrentCamera);
                }
            }
        }

        private void Start()
        {
            if (target == null)
                target = GetComponentInParent<EnemyHealth>();

            // ������� head anchor �������������
            if (headAnchor == null && target != null)
            {
                var go = new GameObject("HeadAnchor");
                go.transform.SetParent(target.transform);
                go.transform.localPosition = Vector3.up * 2f;
                headAnchor = go.transform;
            }

            if (target == null || headAnchor == null || fillImage == null)
            {
                Debug.LogError("[EnemyHealthBarUI] Missing references");
                enabled = false;
                return;
            }

            // ������������� �� ��������
            target.OnHealthChanged += HandleHealthChanged;

            // ���� ������ ��� ����
            if (canvas != null && cam != null)
                canvas.worldCamera = cam;

            HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
        }

        private void LateUpdate()
        {
            uiTimer += Time.deltaTime;
            if (uiTimer < 0.1f) return;
            uiTimer = 0f;

            if (!target || cam == null)
                return;

            // ������� ��� �������
            transform.position = headAnchor.position;

            // face the camera
            transform.LookAt(cam.transform, Vector3.up);

            // progress bar
            fillImage.fillAmount = targetFill;
        }

        private void HandleHealthChanged(float current, float max)
        {
            targetFill = max > 0 ? current / max : 0f;

            if (current <= 0)
                Invoke(nameof(Hide), 1f);
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

        private void OnDisable()
        {
            if (CameraRegistry.Instance != null)
                CameraRegistry.Instance.OnCameraChanged -= HandleCameraChanged;
        }

        private void OnDestroy()
        {
            if (target != null)
                target.OnHealthChanged -= HandleHealthChanged;
        }
    }
}
