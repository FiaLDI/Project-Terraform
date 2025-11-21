using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Target")]
    [SerializeField] private EnemyHealth target;
    [SerializeField] private Transform headAnchor;

    void Hide()
    {
        gameObject.SetActive(false);
    }

    private Camera cam;
    private float targetFill = 1f;
    private const float smooth = 12f;

    private void Start()
    {
        cam = Camera.main;

        if (target == null)
        {
            target = GetComponentInParent<EnemyHealth>();
        }

        if (target == null || headAnchor == null || fillImage == null)
        {
            enabled = false;
            return;
        }

        target.OnHealthChanged += HandleHealthChanged;

        HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
    }

    private void LateUpdate()
    {
        if (!target || !cam)
            return;

        Vector3 dir = transform.position - cam.transform.position;

        transform.position = headAnchor.position;
        transform.rotation = Quaternion.LookRotation(dir);

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smooth);
    }

    private void HandleHealthChanged(float current, float max)
    {
        targetFill = current / max;

        if (current <= 0)
        {
            targetFill = 0;
            Invoke(nameof(Hide), 1f);
        }
    }

    private void OnDestroy()
    {
        if (target != null)
        {
            target.OnHealthChanged -= HandleHealthChanged;
        }
    }
}
