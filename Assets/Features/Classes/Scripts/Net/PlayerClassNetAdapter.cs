using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(PlayerClassController))]
public sealed class PlayerStateNetAdapter : NetworkBehaviour
{
    private PlayerClassController classController;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    public override void OnStartServer()
    {
        base.OnStartServer();
        Cache();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Cache();
    }

    private void Cache()
    {
        if (classController == null)
            classController = GetComponent<PlayerClassController>();
    }

    // =====================================================
    // SERVER API (Единственная точка входа)
    // =====================================================

    /// <summary>
    /// Вызывается ТОЛЬКО на сервере.
    /// Сервер — источник истины.
    /// </summary>
    [Server]
    public void ApplyClass(string classId)
    {
        if (classController == null)
        {
            Debug.LogError("[PlayerStateNetAdapter] PlayerClassController missing", this);
            return;
        }

        // 1️⃣ сервер применяет локально
        classController.ApplyClass(classId);

        // 2️⃣ репликация всем клиентам (включая владельца)
        RpcApplyClass(classId);
    }

    // =====================================================
    // CLIENT REPLICA
    // =====================================================

    [ObserversRpc]
    private void RpcApplyClass(string classId)
    {
        if (classController == null)
            return;

        classController.ApplyClass(classId);
    }
}
