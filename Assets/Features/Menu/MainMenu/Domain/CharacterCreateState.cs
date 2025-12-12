public class CharacterCreateState : IMainMenuState
{
    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.CharacterCreate);
    }

    public void Exit() {}
}
