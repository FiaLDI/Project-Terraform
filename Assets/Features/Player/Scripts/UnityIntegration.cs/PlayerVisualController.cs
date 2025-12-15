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
        Debug.Log($"[PlayerVisual] ApplyVisual id={presetId}");

        var preset = visualLibrary.Find(presetId);

        if (preset == null)
        {
            Debug.LogError($"[PlayerVisual] Visual preset NOT FOUND: {presetId}");
            return;
        }

        Debug.Log($"[PlayerVisual] Spawning model {preset.modelPrefab.name}");

        if (_spawnedModel != null)
            Destroy(_spawnedModel);

        _spawnedModel = Instantiate(preset.modelPrefab, modelRoot);
        _spawnedModel.transform.localPosition = Vector3.zero;
        _spawnedModel.transform.localRotation = Quaternion.identity;
        _spawnedModel.transform.localScale = Vector3.one;

        _animator = _spawnedModel.GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogError("[PlayerVisual] Animator NOT FOUND in model prefab!");
            return;
        }

        if (preset.animator == null)
        {
            Debug.LogError("[PlayerVisual] AnimatorController is NULL!");
            return;
        }

        _animator.runtimeAnimatorController = preset.animator;

        var movement = GetComponent<PlayerMovement>();
        movement?.SetAnimator(_animator);

        Debug.Log("[PlayerVisual] Visual applied successfully");
    }

}
