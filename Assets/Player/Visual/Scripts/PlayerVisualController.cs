using System;
using System.Collections;
using Features.Equipment.UnityIntegration;
using Features.Player.UnityIntegration;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    public RobotVisualLibrarySO visualLibrary;
    [SerializeField] private Transform modelRoot;

    [Header("Death Burst")]
    [SerializeField] private float deathBurstLifetime = 3f;
    [SerializeField] private float deathBurstForce = 4.5f;
    [SerializeField] private float deathBurstUpwardForce = 1.5f;
    [SerializeField] private float deathBurstInheritedVelocityFactor = 0.35f;
    [SerializeField] private float deathBurstRandomTorque = 12f;
    [SerializeField] private float deathBurstDrag = 0.15f;
    [SerializeField] private float deathBurstAngularDrag = 0.05f;
    [SerializeField, Min(0)] private int maxDeathBurstFragments = 32;

    private GameObject _spawnedModel;
    private Animator _animator;
    private GameObject activeDeathBurst;
    private Coroutine deathBurstCleanupRoutine;
    private RobotVisualPresetSO currentPreset;
    private bool deathBurstPlayed;

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

    private void OnDisable()
    {
        ClearActiveDeathBurst();
    }

    private void OnDestroy()
    {
        ClearActiveDeathBurst();
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

        ResetDeathVisualState();
        currentPreset = preset;

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
        GetComponent<EquipmentManager>()?.EquipFromInventory();
        GetComponent<EquipmentManager>()?.ApplySockets(Sockets);
        GetComponent<PlayerCameraController>().SetHead(Sockets.head);

        ApplyLayer(_spawnedModel);
        
    }

    public void PlayDeathBurst(Vector3 inheritedVelocity)
    {
        if (_spawnedModel == null || deathBurstPlayed)
            return;

        ClearActiveDeathBurst();

        var sourceRenderers = _spawnedModel.GetComponentsInChildren<Renderer>(true);
        if (sourceRenderers.Length == 0)
            return;

        deathBurstPlayed = true;
        SetModelRenderersVisible(false);

        var burstSource = ResolveDeathBurstSource();
        if (burstSource == null)
        {
            deathBurstPlayed = false;
            SetModelRenderersVisible(true);
            return;
        }

        var burstRoot = new GameObject($"{name}_DeathBurst");
        burstRoot.transform.SetPositionAndRotation(_spawnedModel.transform.position, Quaternion.identity);
        burstRoot.transform.localScale = Vector3.one;

        var burstClone = Instantiate(burstSource, _spawnedModel.transform.position, _spawnedModel.transform.rotation);
        burstClone.name = $"{burstSource.name}_DeathBurstClone";
        burstClone.transform.localScale = _spawnedModel.transform.lossyScale;

        try
        {
            CopyPoseRecursive(_spawnedModel.transform, burstClone.transform);
            ApplyLayer(burstClone);

            foreach (var animator in burstClone.GetComponentsInChildren<Animator>(true))
                animator.enabled = false;

            var burstRenderers = SelectDeathBurstRenderers(burstClone.GetComponentsInChildren<Renderer>(true));
            foreach (var renderer in burstRenderers)
            {
                if (renderer == null)
                    continue;

                renderer.enabled = true;
                renderer.transform.SetParent(burstRoot.transform, true);
                PrepareFragment(renderer, burstRoot.transform.position, inheritedVelocity);
            }
        }
        finally
        {
            Destroy(burstClone);
        }

        activeDeathBurst = burstRoot;
        deathBurstCleanupRoutine = StartCoroutine(DestroyDeathBurstAfterDelay(burstRoot, deathBurstLifetime));
    }

    public void ResetDeathVisualState()
    {
        deathBurstPlayed = false;

        ClearActiveDeathBurst();

        SetModelRenderersVisible(true);
    }

    private void ClearActiveDeathBurst()
    {
        if (deathBurstCleanupRoutine != null)
        {
            StopCoroutine(deathBurstCleanupRoutine);
            deathBurstCleanupRoutine = null;
        }

        if (activeDeathBurst != null)
        {
            Destroy(activeDeathBurst);
            activeDeathBurst = null;
        }
    }

    private IEnumerator DestroyDeathBurstAfterDelay(GameObject burstRoot, float lifetime)
    {
        if (lifetime > 0f)
            yield return new WaitForSeconds(lifetime);

        if (burstRoot != null)
            Destroy(burstRoot);

        if (activeDeathBurst == burstRoot)
            activeDeathBurst = null;

        deathBurstCleanupRoutine = null;
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

    private void PrepareFragment(Renderer renderer, Vector3 burstOrigin, Vector3 inheritedVelocity)
    {
        var collider = renderer.GetComponent<Collider>();
        if (collider == null)
            collider = CreateFragmentCollider(renderer);

        var rigidbody = renderer.GetComponent<Rigidbody>();
        if (rigidbody == null)
            rigidbody = renderer.gameObject.AddComponent<Rigidbody>();

        rigidbody.mass = 1f;
        rigidbody.linearDamping = deathBurstDrag;
        rigidbody.angularDamping = deathBurstAngularDrag;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.linearVelocity = inheritedVelocity * deathBurstInheritedVelocityFactor;

        Vector3 forceDirection = renderer.bounds.center - burstOrigin;
        if (forceDirection.sqrMagnitude < 0.0001f)
            forceDirection = UnityEngine.Random.onUnitSphere;

        forceDirection = (forceDirection.normalized + UnityEngine.Random.insideUnitSphere * 0.35f).normalized;

        Vector3 burstVelocity =
            forceDirection * deathBurstForce +
            Vector3.up * deathBurstUpwardForce;

        rigidbody.AddForce(burstVelocity, ForceMode.VelocityChange);
        rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * deathBurstRandomTorque, ForceMode.VelocityChange);
    }

    private GameObject ResolveDeathBurstSource()
    {
        if (currentPreset != null)
        {
            if (currentPreset.deathBurstPrefab != null)
                return currentPreset.deathBurstPrefab;

            if (currentPreset.modelPrefab != null)
                return currentPreset.modelPrefab;
        }

        return _spawnedModel;
    }

    private void CopyPoseRecursive(Transform sourceRoot, Transform targetRoot)
    {
        if (sourceRoot == null || targetRoot == null)
            return;

        CopyPoseRecursiveInternal(sourceRoot, targetRoot);
    }

    private void CopyPoseRecursiveInternal(Transform source, Transform targetRoot)
    {
        var target = FindChildRecursive(targetRoot, source.name);
        if (target != null)
        {
            target.localPosition = source.localPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }

        foreach (Transform child in source)
            CopyPoseRecursiveInternal(child, targetRoot);
    }

    private Transform FindChildRecursive(Transform root, string childName)
    {
        if (root.name == childName)
            return root;

        foreach (Transform child in root)
        {
            var result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }

        return null;
    }

    private Collider CreateFragmentCollider(Renderer renderer)
    {
        Bounds bounds;

        if (renderer is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh != null)
        {
            bounds = skinnedMeshRenderer.sharedMesh.bounds;
            var boxCollider = renderer.gameObject.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            return boxCollider;
        }

        var meshFilter = renderer.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            bounds = meshFilter.sharedMesh.bounds;
            var boxCollider = renderer.gameObject.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            return boxCollider;
        }

        var sphereCollider = renderer.gameObject.AddComponent<SphereCollider>();
        sphereCollider.center = renderer.transform.InverseTransformPoint(renderer.bounds.center);
        sphereCollider.radius = Mathf.Max(renderer.bounds.extents.x, renderer.bounds.extents.y, renderer.bounds.extents.z);
        return sphereCollider;
    }

    private Renderer[] SelectDeathBurstRenderers(Renderer[] renderers)
    {
        if (maxDeathBurstFragments <= 0)
            return new Renderer[0];

        if (renderers.Length <= maxDeathBurstFragments)
            return renderers;

        Array.Sort(renderers, (a, b) => GetRendererVolume(b).CompareTo(GetRendererVolume(a)));

        var selected = new Renderer[maxDeathBurstFragments];
        Array.Copy(renderers, selected, selected.Length);
        return selected;
    }

    private static float GetRendererVolume(Renderer renderer)
    {
        if (renderer == null)
            return 0f;

        var size = renderer.bounds.size;
        return size.x * size.y * size.z;
    }

    private void SetModelRenderersVisible(bool visible)
    {
        if (_spawnedModel == null)
            return;

        foreach (var renderer in _spawnedModel.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = visible;
    }

    public void SetLocalModelVisible(bool visible)
    {
        if (!isLocal) return;
        if (_spawnedModel == null || deathBurstPlayed)
            return;

        SetModelRenderersVisible(visible);
    }
}
