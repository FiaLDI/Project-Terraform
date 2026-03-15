using UnityEngine;
using Features.Enemy.Data;
using Features.Player.UnityIntegration;
using Features.Enemy.UnityIntegration;

public class EnemyLODController : MonoBehaviour
{
    public EnemyConfigSO config;

    private Transform anchor;
    private GameObject currentLodGO;

    private Canvas worldCanvas;

    private Animator anim;
    private Rigidbody rb;

    private bool instancingMode = false;

    private int currentLod = -1;
    private float nextUpdateTime;
    private const float UpdateInterval = 0.08f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Anchor для моделей
        anchor = transform.Find("Anchor");
        if (!anchor)
        {
            anchor = new GameObject("Anchor").transform;
            anchor.SetParent(transform);
            anchor.localPosition = Vector3.zero;
        }

        AutoAssignCanvas();

        nextUpdateTime = Time.time + Random.Range(0f, UpdateInterval);
    }

    private void Update()
    {
        if (config == null)
            return;

        if (Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + UpdateInterval;

        var registry = PlayerRegistry.Instance;
        if (registry == null || registry.LocalPlayer == null)
            return;

        Transform playerTf = registry.LocalPlayer.transform;
        float dist = Vector3.Distance(playerTf.position, transform.position);

        // ---------------- CANVAS ----------------
        if (worldCanvas != null)
        {
            bool shouldShow = dist < config.canvasHideDistance;
            if (worldCanvas.enabled != shouldShow)
                worldCanvas.enabled = shouldShow;
        }

        // ---------------- INSTANCING ----------------
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

        // ---------------- LOD ----------------
        float d0 = config.lod0Distance;
        float d1 = config.lod1Distance;

        int newLod;
        if (dist <= d0) newLod = 0;
        else if (dist <= d1) newLod = 1;
        else newLod = 2;

        if (newLod == currentLod)
            return;

        currentLod = newLod;

        SetLOD(newLod);
        HandleLogicByLOD(newLod);
    }

    // -----------------------------------------------------------
    // LOD SWITCH
    // -----------------------------------------------------------

    private void SetLOD(int lod)
    {
        if (currentLodGO != null)
            Destroy(currentLodGO);

        GameObject prefab = lod switch
        {
            0 => config.lod0Prefab,
            1 => config.lod1Prefab,
            _ => config.lod2Prefab
        };

        if (prefab == null)
            return;

        currentLodGO = Instantiate(prefab, anchor);
        currentLodGO.transform.localPosition = Vector3.zero;
        currentLodGO.transform.localRotation = Quaternion.identity;

        anim = currentLodGO.GetComponentInChildren<Animator>();
    }

    // -----------------------------------------------------------
    // ЛОГИКА ПО LOD (оптимизация)
    // -----------------------------------------------------------

    private void HandleLogicByLOD(int lod)
    {
        var actor = GetComponent<EnemyActor>();

        if (actor != null)
        {
            if (lod == 2)
                actor.enabled = false; // далеко → отключаем AI
            else
                actor.enabled = true;
        }

        if (anim != null)
        {
            anim.enabled = (lod == 0); // только на близком LOD
        }

        if (rb != null && config.makeRigidbodyKinematicInInstancing)
        {
            rb.isKinematic = (lod == 2);
        }
    }

    // -----------------------------------------------------------
    // CANVAS
    // -----------------------------------------------------------

    private void AutoAssignCanvas()
    {
        if (config != null && config.worldCanvasPrefab != null)
        {
            GameObject canvasObj = Instantiate(config.worldCanvasPrefab, transform);
            worldCanvas = canvasObj.GetComponent<Canvas>();
            return;
        }

        var canvasTransform = transform.Find("Canvas");
        if (canvasTransform != null)
            worldCanvas = canvasTransform.GetComponent<Canvas>();
    }

    // -----------------------------------------------------------
    // INSTANCING
    // -----------------------------------------------------------

    private void SwitchToInstancing()
    {
        instancingMode = true;

        if (currentLodGO != null)
            currentLodGO.SetActive(false);

        if (anim)
            anim.enabled = false;

        if (rb && config.makeRigidbodyKinematicInInstancing)
            rb.isKinematic = true;
    }

    private void SwitchToNormal()
    {
        instancingMode = false;

        if (currentLodGO != null)
            currentLodGO.SetActive(true);

        if (anim)
            anim.enabled = true;

        if (rb && config.makeRigidbodyKinematicInInstancing)
            rb.isKinematic = false;
    }

    private void SubmitInstancingDraw()
    {
        if (EnemyGPUInstancer.Instance == null || currentLodGO == null)
            return;

        var r = currentLodGO.GetComponentInChildren<Renderer>();
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
