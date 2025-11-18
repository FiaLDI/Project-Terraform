using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    public Slider slider;

    [Header("Target")]
    public EnemyHealth target;           // Скрипт здоровья врага
    public Vector3 offset = new Vector3(0f, 2f, 0f); // над головой

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        // Если не назначили в инспекторе – пробуем найти у родителя
        if (target == null)
            target = GetComponentInParent<EnemyHealth>();

        if (target == null)
        {
            Debug.LogError("EnemyHealthBarUI: EnemyHealth не найден!", this);
            enabled = false;
            return;
        }

        if (!slider)
        {
            slider = GetComponentInChildren<Slider>();
        }

        if (!slider)
        {
            Debug.LogError("EnemyHealthBarUI: Slider не найден!", this);
            enabled = false;
            return;
        }

        slider.maxValue = target.MaxHealth;
        slider.value = target.CurrentHealth;

        target.OnHealthChanged += HandleHealthChanged;
    }

    private void LateUpdate()
    {
        if (!target) return;

        if (!cam)
            cam = Camera.main;

        // Позиция бара — над врагом, с мировым offset
        transform.position = target.transform.position + offset;

        // ВСЕГДА смотрим на камеру и остаёмся вертикальными
        if (cam)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        slider.maxValue = max;
        slider.value = current;

        if (current <= 0f)
            gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (target != null)
            target.OnHealthChanged -= HandleHealthChanged;
    }
}
