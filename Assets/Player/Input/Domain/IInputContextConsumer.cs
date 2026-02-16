using Features.Player;

public interface IInputContextConsumer
{
    void BindInput(PlayerInputContext input);
    void UnbindInput(PlayerInputContext input);
}
