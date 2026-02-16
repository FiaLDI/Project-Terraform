public interface IServerUsable
{
    void ServerPrimaryStart();
    void ServerPrimaryStop();
    void ServerPrimaryHold();

    void ServerSecondaryStart();
    void ServerSecondaryStop();
    void ServerSecondaryHold();

    void ServerReload();
}
