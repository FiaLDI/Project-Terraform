using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIconUI : MonoBehaviour
{
    public Image icon;
    public Image radialFill;      
    public TextMeshProUGUI timerLabel;

    private BuffInstance buff;
    private BuffTooltipTrigger tooltipTrigger;

    private void Awake()
    {
        tooltipTrigger = GetComponent<BuffTooltipTrigger>();
    }

    public void Bind(BuffInstance buff)
    {
        this.buff = buff;

        if (buff.Config.icon != null)
            icon.sprite = buff.Config.icon;

        if (tooltipTrigger != null)
            tooltipTrigger.Bind(buff);

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

        if (float.IsInfinity(remain))
        {
            timerLabel.text = "";
            radialFill.fillAmount = 1f;
            return;
        }

        timerLabel.text = remain < 1f
            ? $"{remain:0.0}"
            : $"{remain:0}";
    }
}
