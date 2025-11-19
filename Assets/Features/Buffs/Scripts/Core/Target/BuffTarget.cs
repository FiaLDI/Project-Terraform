using UnityEngine;

public class BuffTarget : MonoBehaviour, IBuffTarget
{
    private BuffSystem _buffSystem;

    private void Awake()
    {
        _buffSystem = GetComponent<BuffSystem>();

        if (_buffSystem == null)
            _buffSystem = gameObject.AddComponent<BuffSystem>();
    }

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;
    public BuffSystem BuffSystem => _buffSystem;
}
