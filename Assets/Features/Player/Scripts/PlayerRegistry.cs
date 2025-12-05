using System.Collections.Generic;
using UnityEngine;
using Features.Stats.Adapter;
using Features.Abilities.Application;

[DefaultExecutionOrder(-1000)]
public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    public readonly List<GameObject> PlayerTurrets = new();

    /// <summary> Все игроки (для будущего мультиплеера FishNet) </summary>
    public readonly List<GameObject> Players = new();

    /// <summary> Локальный игрок (solo / host) </summary>
    public GameObject LocalPlayer { get; private set; }

    // Адаптеры
    public StatsFacadeAdapter LocalStats { get; private set; }

    public HealthStatsAdapter LocalHealth => LocalStats?.HealthStats;
    public EnergyStatsAdapter LocalEnergy => LocalStats?.EnergyStats;
    public CombatStatsAdapter LocalCombat => LocalStats?.CombatStats;
    public MovementStatsAdapter LocalMovement => LocalStats?.MovementStats;
    public MiningStatsAdapter LocalMining => LocalStats?.MiningStats;

    public AbilityCaster LocalAbilities { get; private set; }

    public StatsFacadeAdapter LocalAdapter => LocalStats;

    public readonly Dictionary<GameObject, List<GameObject>> PlayerOwnedTurrets
        = new Dictionary<GameObject, List<GameObject>>();


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =======================================================================
    // ✔ РЕГИСТРАЦИЯ ИГРОКА
    // =======================================================================
    public void Register(GameObject player, StatsFacadeAdapter statsAdapter)
    {
        if (!player.CompareTag("Player"))
        {
            Debug.Log("IGNORE non-player stats");
            return;
        }
        if (!Players.Contains(player))
            Players.Add(player);

        if (LocalPlayer == null)
            LocalPlayer = player;

        LocalStats = statsAdapter;
        LocalAbilities = player.GetComponent<AbilityCaster>();
    }

    public void RegisterTurret(GameObject ownerPlayer, GameObject turret)
    {
        if (!PlayerOwnedTurrets.ContainsKey(ownerPlayer))
            PlayerOwnedTurrets[ownerPlayer] = new List<GameObject>();

        PlayerOwnedTurrets[ownerPlayer].Add(turret);

        PlayerTurrets.Add(turret);
    }

    public void UnregisterTurret(GameObject ownerPlayer, GameObject turret)
    {
        if (PlayerOwnedTurrets.TryGetValue(ownerPlayer, out var list))
            list.Remove(turret);

        PlayerTurrets.Remove(turret);
    }

}
