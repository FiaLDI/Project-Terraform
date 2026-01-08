using Features.Input;
using Features.Player;
using Features.Player.UnityIntegration;
using FishNet.Object; // Добавил для проверки IsOwner
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AbilityInputHandler : MonoBehaviour, IInputContextConsumer
{
    private PlayerInputContext input;
    private bool bound;

    private InputAction a1, a2, a3, a4, a5;

    // Храним делегаты, чтобы отписка работала корректно
    private System.Action<InputAction.CallbackContext> _cb1, _cb2, _cb3, _cb4, _cb5;

    public void BindInput(PlayerInputContext ctx)
    {
        if (input == ctx) return;
        if (input != null) UnbindInput(input);

        input = ctx;
        if (input == null) return;

        Debug.Log("[AbilityInputHandler] Binding Input...");

        var p = input.Actions.Player;

        Enable(p, "Ability1", "Ability2", "Ability3", "Ability4", "Ability5");

        // Инициализируем делегаты один раз
        _cb1 = _ => TryCast(0);
        _cb2 = _ => TryCast(1);
        _cb3 = _ => TryCast(2);
        _cb4 = _ => TryCast(3);
        _cb5 = _ => TryCast(4);

        a1 = p.FindAction("Ability1", true); a1.performed += _cb1;
        a2 = p.FindAction("Ability2", true); a2.performed += _cb2;
        a3 = p.FindAction("Ability3", true); a3.performed += _cb3;
        a4 = p.FindAction("Ability4", true); a4.performed += _cb4;
        a5 = p.FindAction("Ability5", true); a5.performed += _cb5;

        bound = true;
        Debug.Log("[AbilityInputHandler] Input Bound Successfully");
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (!bound || input != ctx) return;

        // Теперь отписка работает, так как мы используем те же делегаты
        if (a1 != null && _cb1 != null) a1.performed -= _cb1;
        if (a2 != null && _cb2 != null) a2.performed -= _cb2;
        if (a3 != null && _cb3 != null) a3.performed -= _cb3;
        if (a4 != null && _cb4 != null) a4.performed -= _cb4;
        if (a5 != null && _cb5 != null) a5.performed -= _cb5;

        input = null;
        bound = false;
        Debug.Log("[AbilityInputHandler] Input Unbound");
    }

    private void TryCast(int index)
    {
        Debug.Log($"[AbilityInputHandler] TryCast called for slot {index}");

        // 1. Проверяем контекст игрока
        var player = LocalPlayerContext.Player;
        if (player == null)
        {
            Debug.LogError("[AbilityInputHandler] FAIL: LocalPlayerContext.Player is NULL! PlayerRegistry might not be ready.");
            return;
        }

        // 2. Проверяем наличие адаптера
        var net = player.GetComponent<AbilityCasterNetAdapter>();
        if (net == null)
        {
            Debug.LogError($"[AbilityInputHandler] FAIL: AbilityCasterNetAdapter missing on object '{player.name}'");
            return;
        }

        // 3. ПРОВЕРКА ВЛАДЕНИЯ (Критично для FishNet)
        // Пытаемся получить NetworkBehaviour, чтобы узнать, владеем ли мы объектом
        if (net is NetworkBehaviour nb)
        {
            Debug.Log($"[AbilityInputHandler] Target: {player.name} | IsOwner: {nb.IsOwner} | IsClient: {nb.IsClient}");
            
            if (!nb.IsOwner)
            {
                Debug.LogError($"[AbilityInputHandler] BLOCKED: You are trying to cast on '{player.name}', but you are NOT the owner. RPC will fail.");
                // FishNet не даст отправить ServerRpc с объекта, где IsOwner == false
            }
        }
        else
        {
            Debug.LogWarning("[AbilityInputHandler] Adapter does not inherit from NetworkBehaviour? Ownership check skipped.");
        }

        // 4. Вызов
        Debug.Log($"[AbilityInputHandler] Sending Cast({index}) to Adapter...");
        net.Cast(index);
    }

    private static void Enable(InputActionMap map, params string[] names)
    {
        foreach (var n in names)
            map.FindAction(n, true).Enable();
    }
}
