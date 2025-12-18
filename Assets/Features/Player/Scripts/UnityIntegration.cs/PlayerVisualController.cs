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

    public void ApplyVisual(string presetId)
    {
        var preset = visualLibrary.Find(presetId);
        if (preset == null)
            return;

        if (_spawnedModel != null)
            Destroy(_spawnedModel);

        _spawnedModel = Instantiate(preset.modelPrefab, modelRoot);
        _spawnedModel.transform.localPosition = Vector3.zero;
        _spawnedModel.transform.localRotation = Quaternion.identity;
        _spawnedModel.transform.localScale = Vector3.one;

        _animator = _spawnedModel.GetComponentInChildren<Animator>();
        _animator.runtimeAnimatorController = preset.animator;

        Sockets = _spawnedModel.GetComponentInChildren<CharacterSockets>();
        if (Sockets == null)
        {
            Debug.LogError("[PlayerVisual] CharacterSockets NOT FOUND on model!");
        }

        GetComponent<PlayerAnimationController>()?.SetAnimator(_animator);
        GetComponent<EquipmentManager>()?.ApplySockets(Sockets);
    }
}
