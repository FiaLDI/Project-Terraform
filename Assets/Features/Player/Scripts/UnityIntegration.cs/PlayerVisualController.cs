using Features.Equipment.UnityIntegration;
using Features.Player.UnityIntegration;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    public RobotVisualLibrarySO visualLibrary;
    [SerializeField] private Transform modelRoot;

    private GameObject _spawnedModel;
    private Animator _animator;

    public Animator Animator => _animator;
    public CharacterSockets Sockets { get; private set; }

    private void Awake()
    {
        Debug.Log("[PSN-PVC] Awake", this);
    }

    private void Start()
    {
        Debug.Log("[PSN-PVC] Start (READY)", this);
    }

    public void ApplyVisual(string presetId)
    {
        Debug.Log($"[PSN-PlayerVisualController] ApplyVisual({presetId}), parent={gameObject.name}", this);
        
        var preset = visualLibrary.Find(presetId);
        if (preset == null)
        {
            Debug.LogError($"[PlayerVisualController] Visual preset '{presetId}' not found!");
            return;
        }

        if (_spawnedModel != null)
            Destroy(_spawnedModel);

        if (modelRoot == null)
        {
            Debug.LogError("[PlayerVisualController] modelRoot is null!", this);
            return;
        }

        _spawnedModel = Instantiate(preset.modelPrefab, modelRoot);
        _spawnedModel.transform.localPosition = Vector3.zero;
        _spawnedModel.transform.localRotation = Quaternion.identity;
        _spawnedModel.transform.localScale = Vector3.one;

        foreach (var renderer in _spawnedModel.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }

        _animator = _spawnedModel.GetComponentInChildren<Animator>();
        if (_animator != null)
            _animator.runtimeAnimatorController = preset.animator;
        else
            Debug.LogWarning("[PlayerVisualController] Animator not found on model!");

        Sockets = _spawnedModel.GetComponentInChildren<CharacterSockets>();
        if (Sockets == null)
            Debug.LogError("[PlayerVisualController] CharacterSockets NOT FOUND on model!");

        GetComponent<PlayerAnimationController>()?.SetAnimator(_animator);
        GetComponent<EquipmentManager>()?.ApplySockets(Sockets);

        Debug.Log($"[PlayerVisualController] ✅ Visual applied successfully: {presetId}", this);
    }
}
