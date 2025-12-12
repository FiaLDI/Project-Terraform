using Features.Player.UnityIntegration;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    public RobotVisualLibrarySO visualLibrary;
    [SerializeField] private Transform modelRoot;

    private GameObject _spawnedModel;
    private Animator _animator;

    public Animator Animator => _animator;

    public void ApplyVisual(string presetId)
    {
        var preset = visualLibrary.Find(presetId);

        if (preset == null)
        {
            Debug.LogError("[PlayerVisual] Unknown preset: " + presetId);
            return;
        }

        if (_spawnedModel != null)
            Destroy(_spawnedModel);

        _spawnedModel = Instantiate(preset.modelPrefab, modelRoot);

        _animator = _spawnedModel.GetComponentInChildren<Animator>();

        if (_animator == null)
            Debug.LogError("[PlayerVisual] Model has no Animator component!");

        if (preset.animator != null)
            _animator.runtimeAnimatorController = preset.animator;

        var movement = GetComponent<PlayerMovement>();
        movement?.SetAnimator(_animator);
    }
}
