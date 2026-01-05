using System.Collections.Generic;
using UnityEngine;
using Features.Input;

[DefaultExecutionOrder(-90)]
public class UIStackManager : MonoBehaviour
{
    public static UIStackManager I;

    private readonly Stack<IUIScreen> stack = new();

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    // ======================================================
    // STACK API
    // ======================================================

    public void Push(IUIScreen screen)
    {
        if (screen == null)
            return;

        if (stack.Count > 0)
            stack.Peek().Hide();

        stack.Push(screen);
        screen.Show();

        TrySetMode(screen.Mode);
    }

    public void Pop()
    {
        if (stack.Count == 0)
            return;

        stack.Pop().Hide();

        if (stack.Count > 0)
        {
            var top = stack.Peek();
            top.Show();
            TrySetMode(top.Mode);
        }
        else
        {
            TrySetMode(InputMode.Gameplay);
        }
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private void TrySetMode(InputMode mode)
    {
        if (InputModeManager.I == null)
            return;

        if (stack.Count == 0)
        {
            InputModeManager.I.SetMode(InputMode.Gameplay);
            return;
        }

        InputModeManager.I.SetMode(stack.Peek().Mode);
    }

    // ======================================================
    // QUERIES
    // ======================================================

    public IUIScreen Peek()
    {
        return stack.Count > 0 ? stack.Peek() : null;
    }

    public bool IsTop<T>() where T : class, IUIScreen
    {
        return stack.Count > 0 && stack.Peek() is T;
    }

    public void Clear()
    {
        while (stack.Count > 0)
        {
            stack.Pop().Hide();
        }

        TrySetMode(InputMode.Gameplay);
    }


    public bool HasScreens =>
        stack.Count > 0 &&
        stack.Peek().Mode != InputMode.Gameplay;


}
