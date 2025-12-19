using UnityEngine;
using Features.Biomes.UnityIntegration;
using Features.Enemies;
using Features.Enemy.Data;
using Features.Enemy;
using Features.Player.UnityIntegration;

public class EnemyLODController : MonoBehaviour
{
    public EnemyConfigSO config;

    private Renderer lod0;
    private Renderer lod1;
    private Renderer lod2;

    private Canvas worldCanvas;

    private Animator anim;
    private Rigidbody rb;

    private bool instancingMode = false;

    // throttling LOD updates
    private int currentLod = -1;              // -1 = none / instancing
    private float nextUpdateTime;             // когда в следующий раз пересчитывать LOD
    private const float UpdateInterval = 0.08f; // ~12 раз/сек

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        AutoAssignLODRenderers();
        AutoAssignCanvas();

        // разброс апдейтов по времени, чтобы не всё в один кадр
        nextUpdateTime = Time.time + Random.Range(0f, UpdateInterval);
    }

    private void Update()
    {
        if (config == null)
            return;

        // throttling
        if (Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + UpdateInterval;

        var registry = PlayerRegistry.Instance;
        if (registry == null || registry.LocalPlayer == null)
            return;

        Transform playerTf = registry.LocalPlayer.transform;

        float dist = Vector3.Distance(playerTf.position, transform.position);

        // ------------ CANVAS ------------
        if (worldCanvas != null)
        {
            bool shouldShow = dist < config.canvasHideDistance;
            if (worldCanvas.enabled != shouldShow)
                worldCanvas.enabled = shouldShow;
        }

        // ------------ INSTANCING ------------
        bool useInstancing =
            config.useGPUInstancing &&
            dist > config.instancingDistance;

        if (useInstancing)
        {
            if (!instancingMode)
            {
                SwitchToInstancing();
                currentLod = -1;
            }

            SubmitInstancingDraw();
            return;
        }

        if (instancingMode)
        {
            SwitchToNormal();
        }

        // ------------ NORMAL LOD ------------
        float lodScale = EnemyPerformanceManager.Instance != null
            ? EnemyPerformanceManager.Instance.LodScale
            : 1f;

        float d0 = config.lod0Distance * lodScale;
        float d1 = config.lod1Distance * lodScale;

        int newLod;
        if (dist <= d0) newLod = 0;
        else if (dist <= d1) newLod = 1;
        else newLod = 2;

        if (newLod == currentLod)
            return;

        currentLod = newLod;

        if (lod0) lod0.enabled = (newLod == 0);
        if (lod1) lod1.enabled = (newLod == 1);
        if (lod2) lod2.enabled = (newLod == 2);
    }

    // -----------------------------------------------------------
    // AUTO–DISCOVERY
    // -----------------------------------------------------------

    private void AutoAssignLODRenderers()
    {
        Transform root = transform.Find("Model");
        if (!root)
        {
            Debug.LogError("[EnemyLODController] No 'Model' root found!", this);
            return;
        }

        lod0 = FindRenderer(root, "Model_LOD0");
        lod1 = FindRenderer(root, "Model_LOD1");
        lod2 = FindRenderer(root, "Model_LOD2");
    }

    private Renderer FindRenderer(Transform root, string childName)
    {
        var child = root.Find(childName);
        if (!child) return null;

        return child.GetComponentInChildren<Renderer>();
    }

    private void AutoAssignCanvas()
    {
        if (config != null && config.worldCanvasPrefab != null)
        {
            GameObject canvasObj = Instantiate(config.worldCanvasPrefab, transform);
            worldCanvas = canvasObj.GetComponent<Canvas>();

            var bar = canvasObj.GetComponent<EnemyHealthBarUI>();
            if (bar != null)
            {
                var health = GetComponent<EnemyHealth>();
                bar.Target = health;

                Transform anchor = transform.Find("Anchor");
                if (anchor != null)
                    bar.HeadAnchor = anchor;
            }

            return;
        }

        var canvasTransform = transform.Find("Canvas");
        if (canvasTransform != null)
            worldCanvas = canvasTransform.GetComponent<Canvas>();
    }

    // -----------------------------------------------------------
    // INSTANCING MODE
    // -----------------------------------------------------------

    private void SwitchToInstancing()
    {
        instancingMode = true;

        if (lod0) lod0.enabled = false;
        if (lod1) lod1.enabled = false;
        if (lod2) lod2.enabled = false;

        if (config.disableAnimatorInInstancing && anim)
            anim.enabled = false;

        if (config.makeRigidbodyKinematicInInstancing && rb)
            rb.isKinematic = true;
    }

    private void SwitchToNormal()
    {
        instancingMode = false;

        if (config.disableAnimatorInInstancing && anim)
            anim.enabled = true;

        if (config.makeRigidbodyKinematicInInstancing && rb)
            rb.isKinematic = false;
    }

    private void SubmitInstancingDraw()
    {
        if (EnemyGPUInstancer.Instance == null)
            return;

        Renderer r = lod2 ?? lod1 ?? lod0;
        if (!r) return;

        var mf = r.GetComponent<MeshFilter>();
        if (!mf) return;

        Mesh mesh = mf.sharedMesh;
        Material mat = r.sharedMaterial;

        if (!mesh || !mat || !mat.enableInstancing)
            return;

        EnemyInstance inst = new EnemyInstance
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.lossyScale.x,
            color = Color.white
        };

        EnemyGPUInstancer.Instance.AddInstance(
            mesh,
            mat,
            inst,
            r.shadowCastingMode,
            r.receiveShadows,
            gameObject.layer
        );
    }
}
