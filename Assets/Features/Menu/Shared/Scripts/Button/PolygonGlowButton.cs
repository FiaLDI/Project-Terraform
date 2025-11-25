using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public enum ButtonState { Idle, Hover, Selected, Locked }

public class PolygonGlowButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Images")]
    public Image baseImage;       // главный спрайт (разный для каждой кнопки)
    public Image glowImage;       // спрайт с ShaderGraph

    [Header("Colors")]
    public Color idleColor = Color.white;
    public Color selectedColor = new Color(0.7f, 1f, 1f, 1f);
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Glow")]
    public float hoverHighlight = 1f;
    public float selectedHighlight = 0.4f;
    public float fadeSpeed = 6f;

    [Header("Events")]
    public UnityEvent onClick;

    [Header("State")]
    public bool startLocked = false;

    private Material mat;
    private float currentHighlight;
    private float targetHighlight;

    private PolygonGlowButtonGroup group;
    private ButtonState state = ButtonState.Idle;

    private static readonly int HighlightID = Shader.PropertyToID("_Highlight");
    private static readonly int MainSpriteID = Shader.PropertyToID("_MainSprite");

    public bool IsLocked => state == ButtonState.Locked;

    private void Awake()
    {
        // Клонируем материал для этой кнопки
        if (glowImage != null && glowImage.material != null)
        {
            mat = Instantiate(glowImage.material);
            glowImage.material = mat;
        }

        // Устанавливаем текущий спрайт в шейдер
        ApplySpriteToShader();

        ApplyStateVisual();

        if (startLocked)
            SetState(ButtonState.Locked);
    }

    private void Update()
    {
        if (mat == null) return;

        currentHighlight = Mathf.MoveTowards(
            currentHighlight,
            targetHighlight,
            fadeSpeed * Time.deltaTime
        );

        mat.SetFloat(HighlightID, currentHighlight);
    }

    public void SetGroup(PolygonGlowButtonGroup g)
    {
        group = g;
    }

    private void ApplySpriteToShader()
    {
        if (mat == null || baseImage == null) return;

        Texture2D tex = baseImage.sprite != null ? baseImage.sprite.texture : null;
        mat.SetTexture(MainSpriteID, tex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (state == ButtonState.Locked) return;
        if (state != ButtonState.Selected)
            SetState(ButtonState.Hover);

        CursorSystem.SetPointer();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!baseImage) return;

        ApplySpriteToShader();

        if (startLocked)
            SetState(ButtonState.Locked);
        else if (state == ButtonState.Locked)
            SetState(ButtonState.Idle);
    }
#endif

    public void OnPointerExit(PointerEventData eventData)
    {
        if (state == ButtonState.Locked) return;
        if (state != ButtonState.Selected)
            SetState(ButtonState.Idle);

        CursorSystem.SetDefault();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (state == ButtonState.Locked) return;

        group?.OnButtonClicked(this);
        onClick?.Invoke();

        CursorSystem.SetDefault();
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

    private void ApplyStateVisual()
    {
        switch (state)
        {
            case ButtonState.Idle:
                baseImage.color = idleColor;
                targetHighlight = 0f;
                break;

            case ButtonState.Hover:
                baseImage.color = idleColor;
                targetHighlight = hoverHighlight;
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
