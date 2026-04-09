using UnityEngine;
using System;

public sealed class NetworkTickSystem : MonoBehaviour
{
    public static NetworkTickSystem I;

    public const int TickRate = 60;
    public const float TickDelta = 1f / TickRate;

    public static event Action OnTick;

    public int CurrentTick { get; private set; }

    public bool Paused { get; set; }

    private float _accumulator;

    private void Awake()
    {
        I = this;
    }

    private void Update()
    {
        if (Paused)
            return;
            
        _accumulator += Time.deltaTime;

        while (_accumulator >= TickDelta)
        {
            _accumulator -= TickDelta;
            CurrentTick++;
            OnTick?.Invoke();
        }
    }

    public void SetTick(int serverTick)
    {
        CurrentTick = serverTick;
        _accumulator = 0f;
    }
}
