using UnityEngine;
using Features.Biomes.UnityIntegration; // для RuntimeWorldGenerator
using Features.Enemies;

[RequireComponent(typeof(EnemyLODConfig))]
public class EnemyLODController : MonoBehaviour
{
    private EnemyLODConfig _cfg;
    private Transform _player;

    private Animator _animator;
    private Rigidbody _rb;

    private bool _instancingMode;

    private void Awake()
    {
        _cfg = GetComponent<EnemyLODConfig>();
        _animator = GetComponentInChildren<Animator>();
        _rb = GetComponent<Rigidbody>();

        // если рендеры не проставлены руками — попытка найти автоматически
        if (_cfg.lod0Renderer == null)
            _cfg.lod0Renderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        if (RuntimeWorldGenerator.PlayerInstance != null)
            _player = RuntimeWorldGenerator.PlayerInstance.transform;
    }

    private void Update()
    {
        if (_player == null)
        {
            if (RuntimeWorldGenerator.PlayerInstance != null)
                _player = RuntimeWorldGenerator.PlayerInstance.transform;
            else
                return;
        }

        float dist = Vector3.Distance(_player.position, transform.position);

        float lodScale = EnemyPerformanceManager.Instance != null
            ? EnemyPerformanceManager.Instance.LodScale
            : 1f;

        float d0 = _cfg.lod0Distance * lodScale;
        float d1 = _cfg.lod1Distance * lodScale;
        float instDist = _cfg.instancingDistance * lodScale;

        // --- Canvas ---
        if (_cfg.worldCanvas != null)
        {
            bool showCanvas = dist < _cfg.canvasHideDistance;
            if (_cfg.worldCanvas.enabled != showCanvas)
                _cfg.worldCanvas.enabled = showCanvas;
        }

        // --- Выбор LOD / instancing ---
        bool useInstancing = _cfg.useGPUInstancing && dist > instDist;

        if (useInstancing)
        {
            if (!_instancingMode)
                SwitchToInstancingMode();
            SubmitInstancingDraw();
            return;
        }

        if (_instancingMode)
            SwitchToNormalMode();

        // простой переключатель рендереров
        SetRendererEnabled(_cfg.lod0Renderer, dist <= d0);
        SetRendererEnabled(_cfg.lod1Renderer, dist > d0 && dist <= d1);
        SetRendererEnabled(_cfg.lod2Renderer, dist > d1 && dist <= instDist);
    }

    private void SetRendererEnabled(Renderer r, bool enabled)
    {
        if (r == null) return;
        if (r.enabled != enabled)
            r.enabled = enabled;
    }

    private void SwitchToInstancingMode()
    {
        _instancingMode = true;

        SetRendererEnabled(_cfg.lod0Renderer, false);
        SetRendererEnabled(_cfg.lod1Renderer, false);
        SetRendererEnabled(_cfg.lod2Renderer, false);

        if (_cfg.disableAnimatorInInstancing && _animator != null)
            _animator.enabled = false;

        if (_cfg.makeRigidbodyKinematicInInstancing && _rb != null)
            _rb.isKinematic = true;
    }

    private void SwitchToNormalMode()
    {
        _instancingMode = false;

        if (_cfg.disableAnimatorInInstancing && _animator != null)
            _animator.enabled = true;

        if (_cfg.makeRigidbodyKinematicInInstancing && _rb != null)
            _rb.isKinematic = false;
    }

    private void SubmitInstancingDraw()
    {
        if (!_cfg.useGPUInstancing) return;
        if (EnemyGPUInstancer.Instance == null) return;

        // берём самый дальний / дешёвый рендерер (lod2, потом lod1, потом lod0)
        Renderer r = _cfg.lod2Renderer ?? _cfg.lod1Renderer ?? _cfg.lod0Renderer;
        if (r == null) return;

        var meshFilter = r.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        Material mat = r.sharedMaterial;
        if (mesh == null || mat == null) return;

        // материал должен поддерживать instancing
        if (!mat.enableInstancing)
            return;

        EnemyInstance inst = new EnemyInstance
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.lossyScale.x,
            color = Color.white
        };

        EnemyGPUInstancer.Instance.AddInstance(mesh, mat, inst, r.shadowCastingMode, r.receiveShadows, gameObject.layer);
    }
}
