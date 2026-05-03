using Features.Enemy.Data;
using Features.Enemy.Presentation.LOD;
using FishNet.Object;
using Unity.Entities;
using UnityEngine;

public sealed class EnemyVisualController : NetworkBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int AttackTypeHash = Animator.StringToHash("AttackType");

    public Animator animator;
    [SerializeField] private float movingSpeedThreshold = 0.05f;

    private Entity entity;
    private EntityManager em;

    private EnemyAttackHandler attackHandler;
    private EnemyLODView lodView;
    private EnemyEcsRuntimeBinder binder;

    private Vector3 lastPos;
    private bool hasSpeedParam;
    private bool hasIsMovingParam;
    private bool hasAttackParam;
    private bool hasAttackTypeParam;

    private void Awake()
    {
        attackHandler = GetComponent<EnemyAttackHandler>();
        lodView = GetComponent<EnemyLODView>();
        binder = GetComponent<EnemyEcsRuntimeBinder>();

        em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

        if (lodView != null)
            lodView.OnModelChanged += OnModelChanged;

        ResolveCurrentAnimator();
    }

    private void OnDestroy()
    {
        if (lodView != null)
            lodView.OnModelChanged -= OnModelChanged;
    }

    private void OnModelChanged(GameObject model)
    {
        if (model == null)
            return;

        BindAnimator(model.GetComponentInChildren<Animator>());
    }

    private void Update()
    {
        if (entity == Entity.Null || !em.Exists(entity))
        {
            if (binder != null && binder.Entity != Entity.Null && em.Exists(binder.Entity))
                entity = binder.Entity;

            return;
        }

        if (!em.HasComponent<EnemyAttackState>(entity))
            return;

        var attackState = em.GetComponentData<EnemyAttackState>(entity);

        if (attackState.DoAttack)
        {
            attackState.DoAttack = false;
            em.SetComponentData(entity, attackState);

            Vector3 projectileSpawnPos = default;
            Vector3 projectileHitPoint = default;
            bool hasProjectileVisual = IsServer &&
                attackHandler != null &&
                attackHandler.TryGetProjectileVisualShot(out projectileSpawnPos, out projectileHitPoint);

            PlayAttackRpc(
                (int)attackState.Type,
                hasProjectileVisual ? projectileSpawnPos : default,
                hasProjectileVisual ? projectileHitPoint : default,
                hasProjectileVisual
            );

            if (IsServer)
                attackHandler?.TriggerAttack();
        }

        if (animator != null)
        {
            float speed = (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            if (hasSpeedParam)
                animator.SetFloat(SpeedHash, speed);

            if (hasIsMovingParam)
                animator.SetBool(IsMovingHash, speed > movingSpeedThreshold);
        }

        lastPos = transform.position;
    }

    [ObserversRpc]
    private void PlayAttackRpc(int attackType, Vector3 projectileSpawnPos, Vector3 projectileHitPoint, bool playProjectileVisual)
    {
        if (animator != null)
        {
            if (hasAttackTypeParam)
                animator.SetInteger(AttackTypeHash, attackType);

            if (hasAttackParam)
                animator.SetTrigger(AttackHash);
        }

        if (playProjectileVisual)
            attackHandler?.PlayProjectileVisual(projectileSpawnPos, projectileHitPoint);
    }

    private void ApplyAnimatorControllerOverride(Animator targetAnimator)
    {
        if (targetAnimator == null || binder == null)
            return;

        EnemyConfigSO config = binder.Config;
        if (config == null || config.render == null || config.render.animatorController == null)
            return;

        targetAnimator.runtimeAnimatorController = config.render.animatorController;
    }

    private void CacheAnimatorParameters(Animator targetAnimator)
    {
        hasSpeedParam = false;
        hasIsMovingParam = false;
        hasAttackParam = false;
        hasAttackTypeParam = false;

        if (targetAnimator == null)
            return;

        foreach (var parameter in targetAnimator.parameters)
        {
            if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float)
                hasSpeedParam = true;
            else if (parameter.nameHash == IsMovingHash && parameter.type == AnimatorControllerParameterType.Bool)
                hasIsMovingParam = true;
            else if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger)
                hasAttackParam = true;
            else if (parameter.nameHash == AttackTypeHash && parameter.type == AnimatorControllerParameterType.Int)
                hasAttackTypeParam = true;
        }
    }

    private void ResolveCurrentAnimator()
    {
        if (lodView != null && lodView.GetAnimator() != null)
        {
            BindAnimator(lodView.GetAnimator());
            return;
        }

        if (animator != null)
        {
            BindAnimator(animator);
            return;
        }

        BindAnimator(GetComponentInChildren<Animator>());
    }

    private void BindAnimator(Animator targetAnimator)
    {
        animator = targetAnimator;

        if (animator == null)
            return;

        ApplyAnimatorControllerOverride(animator);
        animator.applyRootMotion = false;
        CacheAnimatorParameters(animator);
    }

    public void RefreshAnimatorBinding()
    {
        ResolveCurrentAnimator();
    }
}
