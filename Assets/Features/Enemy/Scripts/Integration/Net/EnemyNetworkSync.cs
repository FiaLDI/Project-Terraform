using FishNet.Object;
using UnityEngine;

public class EnemyNetworkSync : NetworkBehaviour
{
    private Vector3 targetPos;
    private Quaternion targetRot;

    [SerializeField] private float lerpRate = 15f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        targetPos = transform.position;
        targetRot = transform.rotation;
    }

    private void Update()
    {
        if (IsServer) return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            lerpRate * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            lerpRate * Time.deltaTime
        );
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            SendStateObserversRpc(transform.position, transform.rotation);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(Vector3 pos, Quaternion rot)
    {
        if (IsServer) return;

        targetPos = pos;
        targetRot = rot;
    }
}
