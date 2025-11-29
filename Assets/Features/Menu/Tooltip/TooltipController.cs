using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [Header("UI")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text stats;

    private RectTransform rect;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        Hide(true);
    }

    private void Update()
    {
        if (group.alpha <= 0) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 localPos;

        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mousePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPos
        );

        float height = rect.rect.height;
        float width  = rect.rect.width;

        Vector2 offset = new Vector2(0, height / 2 + 50);

        rect.anchoredPosition = localPos + offset;
    }

    public void ShowForItem(Item item)
    {
        icon.sprite = item.icon;
        title.text = item.itemName;
        description.text = item.description;

        var cur = item.currentLevel;

        stats.text = "";

        if (cur > 0)
        {
            var up = item.upgrades[cur - 1];
            stats.text += $"\n<color=#FFD700>Current Upgrade ({cur})</color>\n";
            foreach (var b in up.bonusStats)
                stats.text += $"{b.stat}: +{b.value}\n";
        }

        group.alpha = 1;
    }


    public void ShowAbility(AbilitySO ability, AbilityCaster caster)
    {
        icon.sprite = ability.icon;
        title.text = ability.displayName;
        description.text = ability.description;

        stats.text = "";

        // ------------------------------------
        //   ENERGY COST (с учётом баффов)
        // ------------------------------------
        float baseCost = ability.energyCost;
        float finalCost = baseCost;

        if (caster != null && caster.Energy != null)
            finalCost = caster.Energy.GetActualCost(baseCost);

        if (Mathf.Approximately(baseCost, finalCost))
        {
            stats.text += $"Energy: {finalCost:0}\n";
        }
        else
        {
            stats.text += $"Energy: <b>{finalCost:0}</b>  <color=#888>(was {baseCost:0})</color>\n";
        }

        // ------------------------------------
        //  COOLDOWN
        // ------------------------------------
        stats.text += $"Cooldown: {ability.cooldown:0.0}s\n";

        // ------------------------------------
        //  CAST TYPE/TIME
        // ------------------------------------
        if (ability.castType == AbilityCastType.Channel)
            stats.text += $"Channel Time: {ability.castTime:0.0}s\n";

        group.alpha = 1f;
    }


    public void ShowBuff(BuffInstance buff)
    {
        var config = buff.Config;

        icon.sprite = config.icon;
        title.text = config.displayName;

        description.text = buff.Config.GetDescription();

        stats.text = "";

        if (!float.IsInfinity(config.duration))
            stats.text += $"Duration: {config.duration:0.0}s\n";

        if (config.isDebuff)
            title.color = Color.red;
        else
            title.color = Color.white;

        group.alpha = 1;
    }


    public void Hide(bool instant = false)
    {
        group.alpha = 0;
    }
}
