using UnityEngine;

public class ProceduralSpawnProvider : MonoBehaviour, IPlayerSpawnProvider
{
    [SerializeField] private float rayHeight = 500f;

    public bool TryGetSpawnPoint(out Vector3 position, out Quaternion rotation)
    {
        Vector3 origin = new Vector3(0, rayHeight, 0);

        if (Physics.Raycast(origin, Vector3.down, out var hit, rayHeight * 2f))
        {
            position = hit.point + Vector3.up * 2f;
            rotation = Quaternion.identity;
            return true;
        }

        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }
}
