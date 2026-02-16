using UnityEngine;

public sealed class NetworkTickSystem : MonoBehaviour
{
    public static NetworkTickSystem I;

    public const int TickRate = 60;
    public const float TickDelta = 1f / TickRate;

    public int CurrentTick { get; private set; }

    private float _accumulator;

    private void Awake()
    {
        I = this;
    }

    private void Update()
    {
        _accumulator += Time.deltaTime;

        while (_accumulator >= TickDelta)
        {
            _accumulator -= TickDelta;
            CurrentTick++;
        }
    }
}
