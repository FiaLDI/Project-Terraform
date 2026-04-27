using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonState { Idle, Hover, Selected, Locked }

public class PolygonGlowButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Images")]
    public Image baseImage;
    public Image glowImage;
    public Image Icon;

    [Header("Label")]
    [SerializeField] private TMP_Text Label;

    [Header("Colors")]
    public Color idleColor = Color.white;
    public Color hoverColor = new Color(0.85f, 0.95f, 1f, 1f);
    public Color selectedColor = new Color(0.7f, 1f, 1f, 1f);
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color idleLabelColor = Color.white;
    public Color hoverLabelColor = new Color(0.7f, 1f, 1f, 1f);

    [Header("Glow")]
    public float hoverHighlight = 1f;
    public float selectedHighlight = 0.4f;
    public float fadeSpeed = 6f;

    [Header("Events")]
    public UnityEvent onClick;

    [Header("State")]
    public bool startLocked = false;

    [SerializeField] private bool interactable = true;

    private Material mat;
    private float currentHighlight;
    private float targetHighlight;

    private PolygonGlowButtonGroup group;
    private ButtonState state = ButtonState.Idle;

    private static readonly int HighlightID = Shader.PropertyToID("Highlight");
    private static readonly int MainSpriteID = Shader.PropertyToID("MainSprite");

    public bool IsLocked => state == ButtonState.Locked;
    public bool Interactable => interactable && state != ButtonState.Locked;

    private void Awake()
    {
        if (glowImage != null && glowImage.material != null)
        {
            mat = Instantiate(glowImage.material);
            glowImage.material = mat;
            mat.SetFloat(HighlightID, 0f);
        }

        ApplySpriteToShader();

        if (startLocked)
            SetState(ButtonState.Locked);
        else
            ApplyStateVisual();
    }

    private void Update()
    {
        if (mat == null) return;

        currentHighlight = Mathf.MoveTowards(
            currentHighlight,
            targetHighlight,
            fadeSpeed * Time.unscaledDeltaTime
        );

        mat.SetFloat(HighlightID, currentHighlight);
    }

    public void SetGroup(PolygonGlowButtonGroup g)
    {
        group = g;
    }

    public void SetInteractable(bool value)
    {
        interactable = value;

        if (!interactable && state != ButtonState.Locked)
            SetState(ButtonState.Idle);

        ApplyStateVisual();
    }

    public void SetLocked(bool locked)
    {
        SetState(locked ? ButtonState.Locked : ButtonState.Idle);
    }

    public void SetState(ButtonState newState)
    {
        state = newState;
        ApplyStateVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable) return;

        if (state != ButtonState.Selected)
            SetState(ButtonState.Hover);

        CursorSystem.SetPointer();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactable) return;

        if (state != ButtonState.Selected)
            SetState(ButtonState.Idle);

        CursorSystem.SetDefault();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable) return;

        group?.OnButtonClicked(this);
        onClick?.Invoke();

        CursorSystem.SetDefault();
    }

    private void ApplySpriteToShader()
    {
        if (mat == null || baseImage == null) return;

        Texture2D tex = baseImage.sprite != null ? baseImage.sprite.texture : null;
        mat.SetTexture(MainSpriteID, tex);
    }

    private void ApplyStateVisual()
    {
        if (baseImage == null) return;

        switch (state)
        {
            case ButtonState.Idle:
                baseImage.color = interactable ? idleColor : lockedColor;
                targetHighlight = 0f;
                if (Icon != null)
                    Icon.color = interactable ? idleColor : lockedColor;
                if (Label != null)
                    Label.color = idleLabelColor;
                break;

            case ButtonState.Hover:
                baseImage.color = hoverColor;
                targetHighlight = hoverHighlight;
                if (Icon != null)
                    Icon.color = hoverColor;
                if (Label != null)
                    Label.color = hoverLabelColor;
                break;

            case ButtonState.Selected:
                baseImage.color = selectedColor;
                targetHighlight = selectedHighlight;
                break;

            case ButtonState.Locked:
                baseImage.color = lockedColor;
                targetHighlight = 0f;
                break;
        }
    }
}
