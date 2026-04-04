using UnityEngine;

public sealed class WeaponMuzzleProvider : MonoBehaviour
{
    [SerializeField] private Transform muzzle;

    public Transform Muzzle
    {
        get
        {
            if (muzzle != null)
                return muzzle;

            // авто-поиск если не назначен
            muzzle = FindDeep(transform, "Muzzle");

            if (muzzle == null)
                Debug.LogWarning($"[WeaponMuzzleProvider] Muzzle not found on {name}", this);

            return muzzle;
        }
    }

    private Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name)
                return t;
        }
        return null;
    }
}
