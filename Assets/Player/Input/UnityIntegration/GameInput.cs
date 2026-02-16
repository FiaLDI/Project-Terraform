using UnityEngine.InputSystem;

public sealed class GameInput
{
    private readonly InputActionAsset asset;

    public GameInput(InputActionAsset asset)
    {
        this.asset = asset;
    }

    public InputActionMap Player => asset.FindActionMap("Player", true);
    public InputActionMap UI     => asset.FindActionMap("UI", true);
}
