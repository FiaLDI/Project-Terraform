using UnityEngine;

public interface IPlayerSpawnProvider
{
    bool TryGetSpawnPoint(out Vector3 position, out Quaternion rotation);
}
