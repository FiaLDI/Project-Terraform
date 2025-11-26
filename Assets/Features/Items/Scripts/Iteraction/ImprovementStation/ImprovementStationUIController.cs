using UnityEngine;
using UnityEngine.UI;

public class ImprovementStationUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas canvas;
    public Button closeButton;

    private ImprovementStation owner;


    public void Init(ImprovementStation station)
    {
        owner = station;

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

        Debug.Log("[ImprovementStation UI] Closed");
    }
}
