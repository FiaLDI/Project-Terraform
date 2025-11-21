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
        Debug.Log("HIDE() CALLED — DISABLING HEALTHBAR", this);
        gameObject.SetActive(false);
    }

    private Camera cam;
    private float targetFill = 1f;
    private const float smooth = 12f;

    private void Start()
    {
        Debug.Log("<color=green>EnemyHealthBarUI START</color>", this);

        cam = Camera.main;

        Debug.Log("Camera.main = " + cam);

        if (target == null)
        {
            target = GetComponentInParent<EnemyHealth>();
            Debug.Log("Auto-assigned target = " + target, this);
        }

        Debug.Log("References check:\n" +
                  "target = " + target + "\n" +
                  "headAnchor = " + headAnchor + "\n" +
                  "fillImage = " + fillImage);

        if (target == null || headAnchor == null || fillImage == null)
        {
            Debug.LogError("<color=red>EnemyHealthBarUI: Missing references!</color>", this);
            enabled = false;
            return;
        }

        target.OnHealthChanged += HandleHealthChanged;
        Debug.Log("Subscribed to OnHealthChanged", this);

        HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
        Debug.Log("Initial health update: " + target.CurrentHealth + "/" + target.MaxHealth);
    }

    private void LateUpdate()
    {
        Debug.Log("<color=#8888ff>LateUpdate RUNNING</color>", this);

        if (!target)
        {
            Debug.LogError("LateUpdate: target missing!", this);
            return;
        }

        if (!cam)
        {
            Debug.LogError("LateUpdate: camera missing!", this);
            return;
        }

        Vector3 dir = transform.position - cam.transform.position;

        Debug.Log("fillAmount = " + fillImage.fillAmount +
                  "  targetFill = " + targetFill, this);

        transform.position = headAnchor.position;
        transform.rotation = Quaternion.LookRotation(dir);

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smooth);
        Debug.Log("After Lerp fillAmount = " + fillImage.fillAmount, this);
    }

    private void HandleHealthChanged(float current, float max)
    {
        Debug.Log("<color=yellow>HandleHealthChanged CALLED → " +
                  current + "/" + max + "</color>", this);

        targetFill = current / max;
        Debug.Log("Updated targetFill = " + targetFill, this);

        if (current <= 0)
        {
            Debug.LogWarning("Health reached 0 — scheduling Hide()", this);
            targetFill = 0;
            Invoke(nameof(Hide), 1f);
        }
    }

    private void OnDisable()
    {
        Debug.Log("<color=orange>EnemyHealthBarUI DISABLED</color>", this);
    }

    private void OnDestroy()
    {
        Debug.Log("<color=red>EnemyHealthBarUI DESTROYED</color>", this);

        if (target != null)
        {
            target.OnHealthChanged -= HandleHealthChanged;
            Debug.Log("Unsubscribed from OnHealthChanged");
        }
    }
}
