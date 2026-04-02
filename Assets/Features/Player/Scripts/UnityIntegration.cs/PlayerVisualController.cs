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
    private bool isLocal;
    public CharacterSockets Sockets { get; private set; }

    private void Awake()
    {
        Debug.Log("[PSN-PVC] Awake", this);
    }

    private void Start()
    {
        Debug.Log("[PSN-PVC] Start (READY)", this);
    }

    public void SetLocal(bool value)
    {
        isLocal = value;
    }

    public void ApplyVisual(string presetId)
    {
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
        GetComponent<PlayerCameraController>().SetHead(Sockets.head);

        ApplyLayer(_spawnedModel);
        
    }

    private void ApplyLayer(GameObject _spawnedModel)
    {
        if (_spawnedModel == null)
            return;

        int layer = isLocal
            ? LayerMask.NameToLayer("tps")
            : LayerMask.NameToLayer("Player");

        SetLayerRecursive(_spawnedModel, layer);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    public void SetLocalModelVisible(bool visible)
    {
        if (!isLocal) return;
        if (_spawnedModel == null)
            return;

        foreach (var renderer in _spawnedModel.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = visible;
        }
    }
}
