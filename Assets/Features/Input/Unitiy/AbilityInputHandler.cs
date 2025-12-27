using Features.Input;
using Features.Player;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AbilityInputHandler : MonoBehaviour, IInputContextConsumer
{
    private PlayerInputContext input;
    private bool bound;

    private InputAction a1, a2, a3, a4, a5;

    public void BindInput(PlayerInputContext ctx)
    {
        if (input == ctx) return;
        if (input != null) UnbindInput(input);

        input = ctx;
        if (input == null) return;

        var p = input.Actions.Player;

        Enable(p, "Ability1", "Ability2", "Ability3", "Ability4", "Ability5");

        a1 = p.FindAction("Ability1", true); a1.performed += _ => TryCast(0);
        a2 = p.FindAction("Ability2", true); a2.performed += _ => TryCast(1);
        a3 = p.FindAction("Ability3", true); a3.performed += _ => TryCast(2);
        a4 = p.FindAction("Ability4", true); a4.performed += _ => TryCast(3);
        a5 = p.FindAction("Ability5", true); a5.performed += _ => TryCast(4);

        bound = true;
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (!bound || input != ctx) return;

        if (a1 != null) a1.performed -= _ => TryCast(0); // лучше хранить делегаты, если будешь часто ребиндить
        if (a2 != null) a2.performed -= _ => TryCast(1);
        if (a3 != null) a3.performed -= _ => TryCast(2);
        if (a4 != null) a4.performed -= _ => TryCast(3);
        if (a5 != null) a5.performed -= _ => TryCast(4);

        input = null;
        bound = false;
    }

    private void TryCast(int index)
    {
        var player = LocalPlayerContext.Player;
        if (player == null) return;

        var net = player.GetComponent<AbilityCasterNetAdapter>();
        if (net == null) return;

        net.Cast(index);
    }

    private static void Enable(InputActionMap map, params string[] names)
    {
        foreach (var n in names)
            map.FindAction(n, true).Enable();
    }
}
