using UnityEngine;
using Unity.Entities;
using Features.Enemy.Data;
using Features.Player.UnityIntegration;
using Features.Enemy.UnityIntegration;

public class EnemyLODController : MonoBehaviour
{
    public EnemyConfigSO config;

    private Transform anchor;

    private GameObject[] lodObjects = new GameObject[3];
    private int currentLod = -1;

    private Canvas worldCanvas;
    private Animator anim;

    private EnemyEcsMoveBridge bridge;
    private EnemyAttackHandler attack;

    private Entity entity;
    private EntityManager em;

    private bool instancingMode = false;

    private Mesh instancingMesh;
    private Material instancingMat;

    private float nextUpdateTime;
    private const float UpdateInterval = 0.08f;

    private void Awake()
    {
        anchor = transform.Find("Anchor");
        if (!anchor)
        {
            anchor = new GameObject("Anchor").transform;
            anchor.SetParent(transform);
            anchor.localPosition = Vector3.zero;
        }

        bridge = GetComponent<EnemyEcsMoveBridge>();
        attack = GetComponent<EnemyAttackHandler>();

        AutoAssignCanvas();

        InitLODs();

        nextUpdateTime = Time.time + Random.Range(0f, UpdateInterval);
    }

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder != null)
            entity = binder.Entity;
    }

    private void Update()
    {
        if (config == null) return;
        if (Time.time < nextUpdateTime) return;

        nextUpdateTime = Time.time + UpdateInterval;

        var registry = PlayerRegistry.Instance;
        if (registry == null || registry.LocalPlayer == null)
            return;

        float dist = Vector3.Distance(
            registry.LocalPlayer.transform.position,
            transform.position
        );

        // ---------------- CANVAS ----------------
        if (worldCanvas != null)
            worldCanvas.enabled = dist < config.canvasHideDistance;

        // ---------------- INSTANCING ----------------
        bool useInstancing =
            config.useGPUInstancing &&
            dist > config.instancingDistance;

        if (useInstancing)
        {
            if (!instancingMode)
                EnterInstancing();

            SubmitInstancing();
            return;
        }

        if (instancingMode)
            ExitInstancing();

        // ---------------- LOD ----------------
        int newLod =
            dist <= config.lod0Distance ? 0 :
            dist <= config.lod1Distance ? 1 : 2;

        if (newLod == currentLod)
            return;

        currentLod = newLod;

        ApplyLOD(newLod);
        ApplyLogic(newLod);
    }

    // =========================================================
    // INIT
    // =========================================================

    private void InitLODs()
    {
        lodObjects[0] = Instantiate(config.lod0Prefab, anchor);
        lodObjects[1] = Instantiate(config.lod1Prefab, anchor);
        lodObjects[2] = Instantiate(config.lod2Prefab, anchor);

        for (int i = 0; i < lodObjects.Length; i++)
        {
            lodObjects[i].transform.localPosition = Vector3.zero;
            lodObjects[i].transform.localRotation = Quaternion.identity;
            lodObjects[i].SetActive(false);
        }
    }

    private void AutoAssignCanvas()
    {
        if (config.worldCanvasPrefab != null)
        {
            var obj = Instantiate(config.worldCanvasPrefab, transform);
            worldCanvas = obj.GetComponent<Canvas>();
            return;
        }

        var c = transform.Find("Canvas");
        if (c) worldCanvas = c.GetComponent<Canvas>();
    }

    // =========================================================
    // LOD
    // =========================================================

    private void ApplyLOD(int lod)
    {
        for (int i = 0; i < lodObjects.Length; i++)
            lodObjects[i].SetActive(i == lod);

        anim = lodObjects[lod].GetComponentInChildren<Animator>();

        // кеш для instancing
        if (lod == 2)
        {
            var r = lodObjects[2].GetComponentInChildren<Renderer>();
            if (r != null)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    instancingMesh = mf.sharedMesh;
                    instancingMat = r.sharedMaterial;
                }
            }
        }
    }

    private void ApplyLogic(int lod)
    {
        // ECS active/inactive
        if (em.Exists(entity))
        {
            if (lod >= 2)
            {
                if (!em.HasComponent<EnemyInactive>(entity))
                    em.AddComponent<EnemyInactive>(entity);
            }
            else
            {
                if (em.HasComponent<EnemyInactive>(entity))
                    em.RemoveComponent<EnemyInactive>(entity);
            }
        }

        // movement
        if (bridge != null)
            bridge.enabled = (lod < 2);

        // attack
        if (attack != null)
            attack.enabled = true;

        // animation
        if (anim != null)
            anim.enabled = (lod == 0);
    }

    // =========================================================
    // INSTANCING
    // =========================================================

    private void EnterInstancing()
    {
        instancingMode = true;

        for (int i = 0; i < lodObjects.Length; i++)
            lodObjects[i].SetActive(false);

        if (bridge) bridge.enabled = false;
        if (attack) attack.enabled = false;
    }

    private void ExitInstancing()
    {
        instancingMode = false;

        ApplyLOD(currentLod);
        ApplyLogic(currentLod);
    }

    private void SubmitInstancing()
    {
        if (EnemyGPUInstancer.Instance == null) return;
        if (instancingMesh == null || instancingMat == null) return;

        EnemyGPUInstancer.Instance.AddInstance(
            instancingMesh,
            instancingMat,
            new EnemyInstance
            {
                position = transform.position,
                rotation = transform.rotation,
                scale = transform.lossyScale.x,
                color = Color.white
            },
            UnityEngine.Rendering.ShadowCastingMode.On,
            true,
            gameObject.layer
        );
    }
}
