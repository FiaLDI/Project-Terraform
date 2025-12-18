public interface IInteractable
{
    string InteractionPrompt { get; }
    bool Interact();
}