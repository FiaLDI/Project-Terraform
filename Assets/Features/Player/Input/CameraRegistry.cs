using UnityEngine;
using System;

public class CameraRegistry : MonoBehaviour
{
    public static CameraRegistry I { get; private set; }

    /// <summary>Текущая активная камера клиента.</summary>
    public Camera CurrentCamera { get; private set; }

    /// <summary>Событие смены камеры.</summary>
    public event Action<Camera> OnCameraChanged;

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

    public void RegisterCamera(Camera cam)
    {
        CurrentCamera = cam;
        OnCameraChanged?.Invoke(CurrentCamera);
    }

    public void UnregisterCamera(Camera cam)
    {
        if (cam == CurrentCamera)
        {
            CurrentCamera = null;
            OnCameraChanged?.Invoke(null);
        }
    }
}
