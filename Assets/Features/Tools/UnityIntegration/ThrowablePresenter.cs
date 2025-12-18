using UnityEngine;
using Features.Items.UnityIntegration;
using Features.Equipment.Domain;

public class ThrowablePresenter : MonoBehaviour, IUsable
{
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float upwardForce = 2f;
    [SerializeField] private float torqueForce = 5f;

    private Camera cam;
    private Rigidbody rb;
    private bool thrown;

    // ============================================================
    // INIT
    // ============================================================

    public void Initialize(Camera camera)
    {
        cam = camera;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[ThrowablePresenter] Rigidbody missing");
            enabled = false;
            return;
        }

        // Пока в руках — физика выключена
        rb.isKinematic = true;
        rb.useGravity = false;

        thrown = false;
        enabled = true;
    }

    // ============================================================
    // IUsable
    // ============================================================

    public void OnUsePrimary_Start()
    {
        if (thrown)
            return;

        Throw();
    }

    public void OnUsePrimary_Hold() { }
    public void OnUsePrimary_Stop() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    // ============================================================

    private void Throw()
    {
        thrown = true;

        // Отстыковываем от руки
        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 dir = cam.transform.forward;

        rb.AddForce(
            dir * throwForce + Vector3.up * upwardForce,
            ForceMode.VelocityChange
        );

        rb.AddTorque(
            Random.onUnitSphere * torqueForce,
            ForceMode.VelocityChange
        );

        // После броска — это уже world-item
        enabled = false;
    }
}
