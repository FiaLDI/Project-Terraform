using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Features.Input;
using Features.Game;

public class PauseMenu : MonoBehaviour, IUIScreen
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button returnToHubButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button returnToSpawnButton;

    public InputMode Mode => InputMode.Pause;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvas.enabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        resumeButton.onClick.AddListener(OnResume);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);
        returnToHubButton.onClick.AddListener(onHubReturn);
        returnToSpawnButton.onClick.AddListener(OnReturnToSpawn);
    }

    public void Show()
    {
        canvas.enabled = true;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    private void OnResume()
    {
        UIStackManager.I.Pop();
    }

    private void OnSettings()
    {
        SettingsMenu.I.Open();
    }

    private void OnExit()
    {
        Application.Quit(); 
    }

    private void onHubReturn()
    {
        GetComponentInParent<BootstrapRoot>().LocalPlayer.GetComponent<PlayerNetworkController>().RequestReturnToHubServerRpc();
        
        UIStackManager.I.Clear();
    }

    private void OnReturnToSpawn()
    {
        var conn = FishNet.InstanceFinder.ClientManager.Connection;
        if (conn == null)
            return;

        foreach (var obj in conn.Objects)
        {
            var controller = obj.GetComponent<PlayerNetworkController>();
            if (controller != null && controller.IsOwner)
            {
                controller.RequestReturnToSpawnServerRpc();
                break;
            }
        }

        UIStackManager.I.Pop();
    }
}
