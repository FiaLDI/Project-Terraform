using UnityEngine;

public class UsableToolDrill : MonoBehaviour, IUsable
{
    public float damagePerSecond = 10f;
    public float range = 3f;
    public LayerMask hitMask;

    private Camera cam;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;
    }

    public void OnUsePrimary_Start()
    {
        // сразу же делаем тик урона при старте
        TryDrill();
    }

    public void OnUsePrimary_Hold()
    {
        TryDrill();
    }

    public void OnUsePrimary_Stop() { }

    private void TryDrill()
    {
        Debug.Log("Drill Try"); // уже есть

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            Debug.Log("Ray hit " + hit.collider.name);

            var dmg = hit.collider.GetComponent<IDamageable>();
            if (dmg != null)
            {
                Debug.Log("Found IDamageable → applying damage");
                dmg.TakeDamage(damagePerSecond * Time.deltaTime, DamageType.Mining);
            }
            else Debug.Log("This collider is NOT IDamageable");
        }
        else Debug.Log("NO HIT");
    }


}
