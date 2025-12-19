using System.Collections.Generic;
using UnityEngine;
using Features.Input;

public class UIStackManager : MonoBehaviour
{
    public static UIStackManager I;

    private readonly Stack<IUIScreen> stack = new();

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    public void Push(IUIScreen screen)
    {
        if (stack.Count > 0)
            stack.Peek().Hide();

        stack.Push(screen);
        screen.Show();

        InputModeManager.I.SetMode(screen.Mode);
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
            InputModeManager.I.SetMode(top.Mode);
        }
        else
        {
            InputModeManager.I.SetMode(InputMode.Gameplay);
        }
    }

    public IUIScreen Peek()
    {
        return stack.Count > 0 ? stack.Peek() : null;
    }

    public bool IsTop<T>() where T : class, IUIScreen
    {
        if (stack.Count == 0)
            return false;

        return stack.Peek() is T;
    }

    public bool HasScreens => stack.Count > 0;
}
