using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public sealed class UIRegistry : MonoBehaviour
{
    public static UIRegistry I { get; private set; }

    private readonly Dictionary<Type, IUIScreen> screens = new();

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ======================================================
    // REGISTRATION
    // ======================================================

    public void Register(IUIScreen screen)
    {
        if (screen == null)
            return;

        var type = screen.GetType();
        screens[type] = screen;
    }

    public void Unregister(IUIScreen screen)
    {
        if (screen == null)
            return;

        var type = screen.GetType();

        if (screens.TryGetValue(type, out var current) && current == screen)
            screens.Remove(type);
    }

    // ======================================================
    // QUERY
    // ======================================================

    public T Get<T>() where T : class, IUIScreen
    {
        return screens.TryGetValue(typeof(T), out var screen)
            ? screen as T
            : null;
    }

    public bool Has<T>() where T : class, IUIScreen
    {
        return screens.ContainsKey(typeof(T));
    }
}
