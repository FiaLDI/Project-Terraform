using UnityEngine;
using System;
using Features.Abilities.Domain;
using Features.Abilities.Application;
using Features.Abilities.UnityIntegration;

public class AbilityCaster : MonoBehaviour
{
    [Header("Ability slots (auto from class)")]
    public AbilitySO[] abilities = new AbilitySO[5];

    [Header("Auto refs")]
    public Camera aimCamera;
    public PlayerEnergy energy;
    public LayerMask groundMask;
    public AbilityExecutor executor;

    private AbilityService service;
    private ClassManager classManager;

    public event Action<AbilitySO> OnAbilityCast;
    public event Action<AbilitySO, float, float> OnCooldownChanged;
    public event Action<AbilitySO> OnChannelStarted;
    public event Action<AbilitySO, float, float> OnChannelProgress;
    public event Action<AbilitySO> OnChannelCompleted;
    public event Action<AbilitySO> OnChannelInterrupted;

    private void Awake()
    {
        if (energy == null)
            energy = GetComponent<PlayerEnergy>();

        classManager = FindAnyObjectByType<ClassManager>();

        AutoDetectCamera();

        // пока executor может быть null — это норм
        executor = AbilityExecutor.I;

        service = new AbilityService(
            owner: gameObject,
            aimCamera: aimCamera,
            energy: energy,
            groundMask: groundMask,
            executor: executor
        );

        // events
        service.OnAbilityCast += a => OnAbilityCast?.Invoke(a);
        service.OnCooldownChanged += (a, r, m) => OnCooldownChanged?.Invoke(a, r, m);
        service.OnChannelStarted += a => OnChannelStarted?.Invoke(a);
        service.OnChannelProgress += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
        service.OnChannelCompleted += a => OnChannelCompleted?.Invoke(a);
        service.OnChannelInterrupted += a => OnChannelInterrupted?.Invoke(a);
    }

    private void Start()
    {
        if (classManager != null)
        {
            classManager.OnAbilitiesChanged += UpdateAbilities;
            UpdateAbilities();
        }
    }

    private void OnDestroy()
    {
        if (classManager != null)
            classManager.OnAbilitiesChanged -= UpdateAbilities;
    }

    private void LateUpdate()
    {
        // главное: если Executor появился после игрока — подцепляем
        if (executor == null && AbilityExecutor.I != null)
        {
            executor = AbilityExecutor.I;
            service.SetExecutor(executor);

            Debug.Log("[AbilityCaster] Late-linked AbilityExecutor");
        }

        service?.Tick(Time.deltaTime);
    }

    private void AutoDetectCamera()
    {
        if (aimCamera != null) return;

        if (CameraRegistry.I?.CurrentCamera != null)
        {
            aimCamera = CameraRegistry.I.CurrentCamera;
            return;
        }

        var cross = FindAnyObjectByType<CrosshairController>();
        if (cross?.cam != null)
        {
            aimCamera = cross.cam;
            return;
        }

        aimCamera = Camera.main;
    }

    private void UpdateAbilities()
    {
        if (classManager == null || classManager.ActiveAbilities == null) return;

        for (int i = 0; i < 5; i++)
            abilities[i] = classManager.ActiveAbilities[i];
    }

    public void TryCast(int index)
    {
        if (index < 0 || index >= abilities.Length) return;
        var ab = abilities[index];
        if (ab == null) return;

        service.TryCast(ab, index);
    }

    public float GetCooldown(int index)
    {
        if (index < 0 || index >= abilities.Length) return 0;
        return service.GetCooldownRemaining(abilities[index]);
    }

    public bool IsChanneling => service.IsChanneling;
    public AbilitySO CurrentChannelAbility => service.CurrentChannelAbility;
}
