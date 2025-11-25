using UnityEngine;
using System;
using System.Runtime.InteropServices;

public static class CursorSystem
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    private const int IDC_ARROW = 32512;
    private const int IDC_HAND = 32649;

#endif

    public static void SetPointer()
    {
#if UNITY_EDITOR
        Cursor.SetCursor(UnityEditor.EditorGUIUtility.IconContent("d_ViewToolMove").image as Texture2D, Vector2.zero, CursorMode.Auto);
#elif UNITY_STANDALONE_WIN
        IntPtr cursor = LoadCursor(IntPtr.Zero, IDC_HAND);
        SetCursor(cursor);
#endif
    }

    public static void SetDefault()
    {
#if UNITY_EDITOR
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
#elif UNITY_STANDALONE_WIN
        IntPtr cursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
        SetCursor(cursor);
#endif
    }
}
