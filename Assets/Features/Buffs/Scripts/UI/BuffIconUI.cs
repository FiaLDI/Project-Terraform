using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIconUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public Image radialFill;      
    public TextMeshProUGUI timerLabel;

    private BuffInstance buff;

    public void Bind(BuffInstance buff)
    {
        this.buff = buff;

        if (buff.Config.icon != null)
            icon.sprite = buff.Config.icon;

        UpdateUI();
    }

    private void Update()
    {
        if (buff == null)
            return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        float remain = buff.Remaining;

        radialFill.fillAmount = buff.Progress01;

        timerLabel.text = remain < 1f
            ? $"{remain:0.0}"
            : $"{remain:0}";
    }
}
