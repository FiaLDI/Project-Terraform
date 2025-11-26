using UnityEngine;
using UnityEngine.UI;

public class WorkbenchUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas canvas;
    public Button closeButton;

    private Workbench owner;


    public void Init(Workbench workbench)
    {
        owner = workbench;

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

        Debug.Log("[Workbench UI] Closed");
    }
}
