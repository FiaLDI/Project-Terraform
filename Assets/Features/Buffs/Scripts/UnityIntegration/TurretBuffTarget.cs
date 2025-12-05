using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;

public class TurretBuffTarget : MonoBehaviour, IBuffTarget
{
    private BuffSystem _system;
    private BuffService _service;

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;
    public BuffSystem BuffSystem => _system;
    public BuffService Buffs => _service;

    private void Awake()
    {
        // обязательно на том же объекте
        _system = GetComponent<BuffSystem>();
        if (_system == null)
            Debug.LogError("[TurretBuffTarget] BuffSystem not found!");

        _service = _system?.GetServiceSafe();
    }
}
