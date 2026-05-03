using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed class CampaignProgressService : MonoBehaviour
{
    public static CampaignProgressService I { get; private set; }

    private const string FileName = "campaign_progress.json";

    public CampaignSaveData Data { get; private set; }
    public ExpeditionSaveData ActiveExpedition { get; private set; }

    public event Action<ExpeditionSaveData> ActiveExpeditionChanged;
    public event Action ExpeditionsChanged;

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    public int ShipLevel => ActiveExpedition != null
        ? Mathf.Max(1, ActiveExpedition.shipLevel)
        : 1;

    public static CampaignProgressService EnsureExists()
    {
        if (I != null)
            return I;

        var existing = FindFirstObjectByType<CampaignProgressService>();
        if (existing != null)
            return existing;

        var go = new GameObject(nameof(CampaignProgressService));
        return go.AddComponent<CampaignProgressService>();
    }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
        LoadOrCreate();
    }

    public void LoadOrCreate()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            Data = JsonUtility.FromJson<CampaignSaveData>(json);
        }

        if (Data == null)
            Data = new CampaignSaveData();

        NormalizeData();

        ActiveExpedition = GetExpeditionById(Data.activeExpeditionId);
        TouchActiveExpedition();
    }

    public void Save()
    {
        if (Data == null)
            Data = new CampaignSaveData();

        NormalizeData();
        TouchActiveExpedition();

        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("[Campaign] Save failed: " + e);
        }
    }

    public IReadOnlyList<ExpeditionSaveData> GetExpeditions()
    {
        if (Data == null)
            LoadOrCreate();

        NormalizeData();
        return Data.expeditions;
    }

    public ExpeditionSaveData GetExpeditionById(string expeditionId)
    {
        if (Data == null || Data.expeditions == null || string.IsNullOrWhiteSpace(expeditionId))
            return null;

        return Data.expeditions.FirstOrDefault(x => x != null && x.expeditionId == expeditionId);
    }

    public ExpeditionSaveData CreateExpedition(string displayName, string startingPlanetId = "")
    {
        if (Data == null)
            LoadOrCreate();

        var expedition = new ExpeditionSaveData
        {
            expeditionId = Guid.NewGuid().ToString(),
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? GetNextDefaultExpeditionName()
                : displayName.Trim(),
            shipLevel = 1,
            activePlanetId = startingPlanetId ?? string.Empty,
            lastPlayedUtc = DateTime.UtcNow.ToString("O")
        };

        Data.expeditions.Add(expedition);
        SetActiveExpedition(expedition);
        ExpeditionsChanged?.Invoke();
        return expedition;
    }

    public ExpeditionSaveData EnsureActiveExpedition(string defaultPlanetId = "")
    {
        if (Data == null)
            LoadOrCreate();

        if (ActiveExpedition == null)
        {
            if (Data.expeditions != null && Data.expeditions.Count > 0)
                SetActiveExpedition(Data.expeditions[0]);
            else
                return CreateExpedition(string.Empty, defaultPlanetId);
        }

        if (ActiveExpedition != null &&
            string.IsNullOrWhiteSpace(ActiveExpedition.activePlanetId) &&
            !string.IsNullOrWhiteSpace(defaultPlanetId))
        {
            SetActivePlanet(defaultPlanetId);
        }

        return ActiveExpedition;
    }

    public bool DeleteExpedition(string expeditionId)
    {
        if (Data == null || Data.expeditions == null)
            return false;

        int index = Data.expeditions.FindIndex(x => x != null && x.expeditionId == expeditionId);
        if (index < 0)
            return false;

        bool deletedActive = ActiveExpedition != null && ActiveExpedition.expeditionId == expeditionId;
        Data.expeditions.RemoveAt(index);

        if (deletedActive)
        {
            ActiveExpedition = Data.expeditions.Count > 0 ? Data.expeditions[0] : null;
            Data.activeExpeditionId = ActiveExpedition != null ? ActiveExpedition.expeditionId : string.Empty;
            ActiveExpeditionChanged?.Invoke(ActiveExpedition);
        }

        ExpeditionsChanged?.Invoke();
        Save();
        return true;
    }

    public void SetActiveExpedition(ExpeditionSaveData save)
    {
        if (Data == null)
            LoadOrCreate();

        ActiveExpedition = save;
        Data.activeExpeditionId = save != null ? save.expeditionId : string.Empty;
        TouchActiveExpedition();
        ActiveExpeditionChanged?.Invoke(ActiveExpedition);
        Save();
    }

    public void SetActivePlanet(string planetId)
    {
        if (ActiveExpedition == null)
            return;

        ActiveExpedition.activePlanetId = planetId ?? string.Empty;
        TouchActiveExpedition();
        Save();
    }

    public void SetShipLevel(int shipLevel)
    {
        if (ActiveExpedition == null)
            return;

        ActiveExpedition.shipLevel = Mathf.Max(1, shipLevel);
        TouchActiveExpedition();
        Save();
    }

    public PlanetProgressData GetOrCreatePlanetProgress(string planetId)
    {
        if (ActiveExpedition == null || string.IsNullOrWhiteSpace(planetId))
            return null;

        if (ActiveExpedition.planets == null)
            ActiveExpedition.planets = new List<PlanetProgressData>();

        PlanetProgressData planet = ActiveExpedition.planets
            .FirstOrDefault(x => x != null && x.planetId == planetId);

        if (planet != null)
            return planet;

        planet = new PlanetProgressData
        {
            planetId = planetId
        };

        ActiveExpedition.planets.Add(planet);
        TouchActiveExpedition();
        return planet;
    }

    public BiomeThreatProgressData GetOrCreateBiomeProgress(string planetId, string biomeId)
    {
        PlanetProgressData planet = GetOrCreatePlanetProgress(planetId);
        if (planet == null || string.IsNullOrWhiteSpace(biomeId))
            return null;

        if (planet.biomeThreats == null)
            planet.biomeThreats = new List<BiomeThreatProgressData>();

        BiomeThreatProgressData biome = planet.biomeThreats
            .FirstOrDefault(x => x != null && x.biomeId == biomeId);

        if (biome != null)
            return biome;

        biome = new BiomeThreatProgressData
        {
            biomeId = biomeId,
            maxUnlockedThreatLevel = 1
        };

        planet.biomeThreats.Add(biome);
        TouchActiveExpedition();
        return biome;
    }

    public int GetMaxUnlockedThreat(string planetId, string biomeId)
    {
        BiomeThreatProgressData progress = GetOrCreateBiomeProgress(planetId, biomeId);
        return progress != null
            ? Mathf.Max(1, progress.maxUnlockedThreatLevel)
            : 1;
    }

    public void CompleteBiomeThreat(string planetId, string biomeId, int threatLevel, int cap)
    {
        BiomeThreatProgressData biome = GetOrCreateBiomeProgress(planetId, biomeId);
        if (biome == null)
            return;

        int normalizedThreatLevel = Mathf.Max(1, threatLevel);
        if (!biome.completedThreatLevels.Contains(normalizedThreatLevel))
            biome.completedThreatLevels.Add(normalizedThreatLevel);

        if (normalizedThreatLevel >= biome.maxUnlockedThreatLevel)
            biome.maxUnlockedThreatLevel = Mathf.Min(normalizedThreatLevel + 1, Mathf.Max(1, cap));

        TouchActiveExpedition();
        Save();
    }

    public bool TryUnlockPlanetMission(PlanetConfig planet)
    {
        if (!CampaignCatalogUtility.CanUnlockPlanetMission(planet, this))
            return false;

        PlanetProgressData progress = GetOrCreatePlanetProgress(planet.planetId);
        if (progress == null || progress.isPlanetMissionUnlocked)
            return false;

        progress.isPlanetMissionUnlocked = true;
        TouchActiveExpedition();
        Save();
        return true;
    }

    public void MarkPlanetMissionCompleted(string planetId)
    {
        PlanetProgressData progress = GetOrCreatePlanetProgress(planetId);
        if (progress == null)
            return;

        progress.isPlanetMissionCompleted = true;
        TouchActiveExpedition();
        Save();
    }

    private void TouchActiveExpedition()
    {
        if (ActiveExpedition == null)
            return;

        ActiveExpedition.lastPlayedUtc = DateTime.UtcNow.ToString("O");
    }

    private void NormalizeData()
    {
        if (Data == null)
        {
            Data = new CampaignSaveData();
            return;
        }

        if (Data.expeditions == null)
            Data.expeditions = new List<ExpeditionSaveData>();

        for (int i = 0; i < Data.expeditions.Count; i++)
        {
            ExpeditionSaveData expedition = Data.expeditions[i];
            if (expedition == null)
                continue;

            if (expedition.planets == null)
                expedition.planets = new List<PlanetProgressData>();

            expedition.shipLevel = Mathf.Max(1, expedition.shipLevel);

            for (int j = 0; j < expedition.planets.Count; j++)
            {
                PlanetProgressData planet = expedition.planets[j];
                if (planet == null)
                    continue;

                if (planet.biomeThreats == null)
                    planet.biomeThreats = new List<BiomeThreatProgressData>();

                for (int k = 0; k < planet.biomeThreats.Count; k++)
                {
                    BiomeThreatProgressData biome = planet.biomeThreats[k];
                    if (biome == null)
                        continue;

                    biome.maxUnlockedThreatLevel = Mathf.Max(1, biome.maxUnlockedThreatLevel);

                    if (biome.completedThreatLevels == null)
                        biome.completedThreatLevels = new List<int>();
                }
            }
        }
    }

    private string GetNextDefaultExpeditionName()
    {
        if (Data == null || Data.expeditions == null || Data.expeditions.Count == 0)
            return "Expedition 01";

        int maxIndex = 0;

        foreach (ExpeditionSaveData expedition in Data.expeditions)
        {
            if (expedition == null || string.IsNullOrWhiteSpace(expedition.displayName))
                continue;

            const string prefix = "Expedition ";
            if (!expedition.displayName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            string numberPart = expedition.displayName.Substring(prefix.Length).Trim();
            if (int.TryParse(numberPart, out int value))
                maxIndex = Mathf.Max(maxIndex, value);
        }

        return $"Expedition {maxIndex + 1:00}";
    }
}
