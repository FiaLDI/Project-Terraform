using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingProgressUI : MonoBehaviour
{
    [Header("References")]
    public GameObject root;
    public Image fillBar;
    public Image backFillBar; 
    public TextMeshProUGUI progressText;
    public Animator animator;

    [Header("Settings")]
    public float smoothSpeed = 6f;

    private float currentFill = 0f;
    private float targetFill = 0f;

    private bool active = false;


    private void Update()
    {
        if (!active) return;

        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);

        if (fillBar != null)
            fillBar.fillAmount = currentFill;

        if (backFillBar != null)
        {
            float back = Mathf.Lerp(backFillBar.fillAmount, currentFill, Time.deltaTime * (smoothSpeed * 0.4f));
            backFillBar.fillAmount = back;
        }

        UpdateColor(currentFill);
    }


    // =============================
    // PUBLIC API
    // =============================

    public void SetVisible(bool value)
    {
        active = value;
        root.SetActive(value);

        if (!value)
        {
            currentFill = 0;
            targetFill = 0;

            if (fillBar != null) fillBar.fillAmount = 0;
            if (backFillBar != null) backFillBar.fillAmount = 0;
            if (progressText != null) progressText.text = "0%";
        }
    }


    public void UpdateProgress(float t)
    {
        t = Mathf.Clamp01(t);
        targetFill = t;

        if (progressText != null)
            progressText.text = Mathf.RoundToInt(t * 100f) + "%";
    }


    // =============================
    // VISUAL STYLE (цвета как HP/Energy)
    // =============================

    private void UpdateColor(float t)
    {
        if (fillBar == null) return;

        // Цветовая логика как в HP/Energy стилях
        Color targetColor;

        if (t <= 0f)
            targetColor = new Color(0.4f, 0.4f, 0.4f, 1f); // серый
        else if (t < 0.5f)
            targetColor = new Color(1f, 0.82f, 0.35f, 1f); // желтоватый
        else
            targetColor = new Color(0.3f, 0.9f, 1f, 1f); // голубой sci-fi

        fillBar.color = targetColor;
    }
}
