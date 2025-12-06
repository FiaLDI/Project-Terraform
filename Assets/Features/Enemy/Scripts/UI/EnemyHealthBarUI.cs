using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace Features.Enemy
{
    [RequireComponent(typeof(Canvas))]
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private EnemyHealth target; 
        [SerializeField] private Transform headAnchor;

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

        private Camera cam;
        private Canvas canvas;
        private float targetFill = 1f;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }

        private void OnEnable()
        {
            if (CameraRegistry.I != null)
            {
                CameraRegistry.I.OnCameraChanged += HandleCameraChanged;
                if (CameraRegistry.I.CurrentCamera != null)
                {
                    HandleCameraChanged(CameraRegistry.I.CurrentCamera);
                }
            }
            else
            {
            }
        }

        private void Start()
        {
            if (target == null)
            {
                target = GetComponentInParent<EnemyHealth>();
            }

            if (headAnchor == null && target != null)
            {
                var go = new GameObject("HeadAnchor");
                go.transform.SetParent(target.transform);
                go.transform.localPosition = Vector3.up * 2f;
                headAnchor = go.transform;
            }

            if (target == null || headAnchor == null || fillImage == null)
            {
                enabled = false;
                return;
            }

            target.OnHealthChanged += HandleHealthChanged;
            TryAssignFallbackCamera();
            HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
        }

        private void LateUpdate()
        {
            uiTimer += Time.deltaTime;
                if (uiTimer < 0.1f) return;
                uiTimer = 0f;
            if (!target)
            {
                return;
            }

            if (cam == null)
            {
                TryAssignFallbackCamera();
                if (cam == null) return;
            }

            transform.position = headAnchor.position;
            transform.LookAt(cam.transform);

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

        private void HandleCameraChanged(Camera newCam)
        {
            cam = newCam;
            canvas.worldCamera = cam;
        }


        private void OnDestroy()
        {
            if (target != null)
                target.OnHealthChanged -= HandleHealthChanged;
        }

        private void TryAssignFallbackCamera()
        {
            if (cam == null)
            {
                var main = Camera.main;
                if (main != null)
                {
                    cam = main;
                    canvas.worldCamera = cam;
                }
            }
        }

    }
}