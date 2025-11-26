using UnityEngine;
using UnityEngine.UI;

public class MaterialProcessorUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas canvas;
    public Button closeButton;

    private MaterialProcessor owner;


    public void Init(MaterialProcessor processor)
    {
        owner = processor;

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUI);

        if (canvas != null)
            canvas.enabled = false;
    }


    public void SetVisible(bool value)
    {
        if (canvas != null)
            canvas.enabled = value;
    }


    public void CloseUI()
    {
        SetVisible(false);

        PlayerUsageController.InteractionLocked = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[MaterialProcessor UI] Closed");
    }
}
