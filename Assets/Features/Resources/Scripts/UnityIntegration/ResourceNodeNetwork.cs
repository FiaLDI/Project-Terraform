using Features.Resources.UnityIntegration;
using FishNet.Object;
using UnityEngine;

public class ResourceNodeNetwork : NetworkBehaviour
{
    private ResourceNodePresenter presenter;

    private void Awake()
    {
        presenter = GetComponent<ResourceNodePresenter>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void Mine_Server(float amount, float toolMultiplier)
    {
        if (presenter != null)
        {
            presenter.ApplyMining(amount, toolMultiplier);
            
            if (presenter.IsDepleted())
            {
                ServerSpawnDrops();
                SpawnDrops_Clients();
            }
            else
            {
                float currentHealth = presenter.GetCurrentHealth();
                OnMined_Clients(currentHealth);
            }
        }
    }

    [Server]
    private void ServerSpawnDrops()
    {
        if (presenter?.config?.drops == null) return;
        
        var drops = presenter.RollDrops();
        foreach (var item in drops)
        {
            WorldItemDropService.I.DropServer(item, transform.position + Vector3.up, Vector3.forward);
        }
        
        Despawn();
    }

    [ObserversRpc]
    private void SpawnDrops_Clients()
    {
        if (presenter != null)
        {
            presenter.OnDepletedVisual();
        }
    }

    [ObserversRpc]
    private void OnMined_Clients(float health)
    {
        if (presenter != null)
        {
            presenter.SetHealthVisual(health);
        }
    }
}
