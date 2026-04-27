using FishNet.Object;
using Features.Effects.Application;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(SpawnedObjectContext))]
    public sealed class MinerDetonatorBehaviour : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            var context = GetComponent<SpawnedObjectContext>();
            MinerMineBehaviour.DetonateOwnedMines(context != null ? context.Source : null);

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn();
            else
                Destroy(gameObject);
        }
    }
}
