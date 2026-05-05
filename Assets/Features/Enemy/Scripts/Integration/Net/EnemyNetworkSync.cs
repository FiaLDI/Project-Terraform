using FishNet.Object;
using UnityEngine;

public class EnemyNetworkSync : NetworkBehaviour
{
    private Vector3 targetPos;
    private Quaternion targetRot;
    private Vector3 lastSentPos;
    private Quaternion lastSentRot;
    private float sendTimer;

    [SerializeField] private float lerpRate = 15f;
    [SerializeField] private float sendInterval = 0.1f;
    [SerializeField] private float positionThreshold = 0.05f;
    [SerializeField] private float rotationThreshold = 2f;

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
        if (!IsServer)
            return;

        sendTimer -= Time.fixedDeltaTime;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        bool movedEnough = Vector3.Distance(lastSentPos, pos) >= positionThreshold;
        bool rotatedEnough = Quaternion.Angle(lastSentRot, rot) >= rotationThreshold;

        if (sendTimer > 0f && !movedEnough && !rotatedEnough)
            return;

        sendTimer = Mathf.Max(0.02f, sendInterval);
        lastSentPos = pos;
        lastSentRot = rot;
        SendStateObserversRpc(pos, rot);
    }

    [ObserversRpc(BufferLast = true)]
    private void SendStateObserversRpc(Vector3 pos, Quaternion rot)
    {
        if (IsServer) return;

        targetPos = pos;
        targetRot = rot;
    }
}
