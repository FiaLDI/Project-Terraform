using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;
using FishNet;
using Features.Stats.UnityIntegration;
using System;

public class PlayerSessionNetwork : NetworkBehaviour
{
    [ServerRpc]
    public void RequestReturnToSpawnServerRpc()
    {
        int clientId = Owner != null ? Owner.ClientId : -1;
        var registry = PlayerSpawnRegistry.I;

        if (clientId < 0 || registry == null || !registry.TryGetSpawnPoint(clientId, out var pos, out var rot))
            return;

        var movement = GetComponent<DeterministicMovement>();

        movement.ApplyState(new PlayerState
        {
            Tick = InstanceFinder.TimeManager.Tick,
            Position = pos,
            Velocity = Vector3.zero,
            Yaw = rot.eulerAngles.y,
            Pitch = 0f,
            VerticalVelocity = 0f,
            InternalYaw = rot.eulerAngles.y,
            Grounded = true,
            Crouch = false
        });
    }

    [ServerRpc]
    public void RequestReturnToHubServerRpc()
    {
        if (SceneTransitionService.IsLocalSceneActive(SceneTransitionService.NameHubScene) &&
            !SceneTransitionService.IsTransitionPendingFor(SceneTransitionService.NameHubScene))
        {
            Debug.Log("[PlayerSessionNetwork] Return to hub ignored: already in hub.");
            return;
        }

        ShowHubLoadingObserversRpc();
        SceneTransitionService.ReturnAllPlayersToHub();
    }

    [ServerRpc]
    public void RequestOnlinePlayersServerRpc()
    {
        var sessions = ServerCompositionRoot.I?.Sessions;

        if (Owner == null)
            return;

        if (sessions == null)
        {
            TargetReceiveOnlinePlayers(Owner, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<int>());
            return;
        }

        var nicknames = new List<string>();
        var classIds = new List<string>();
        var levels = new List<int>();

        foreach (var session in sessions.GetOnlineSessions())
        {
            nicknames.Add(session.Nickname ?? string.Empty);
            classIds.Add(session.ClassId ?? string.Empty);
            levels.Add(session.Level);
        }

        TargetReceiveOnlinePlayers(Owner, nicknames.ToArray(), classIds.ToArray(), levels.ToArray());
    }

    [ServerRpc]
    public void RequestWorldServerRpc(
        string worldConfigId,
        int difficulty,
        List<string> questIds,
        List<string> chainIds)
    {
        if (SceneTransitionService.IsTransitionPendingFor(SceneTransitionService.NameWorldScene))
        {
            Debug.LogWarning("[PlayerSessionNetwork] Duplicate world request ignored.");
            return;
        }

        int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        var sessions = ServerCompositionRoot.I?.Sessions;
        var requesterSession = sessions?.GetSessionByClient(Owner.ClientId);
        int worldLevel = requesterSession != null ? requesterSession.Level : 1;
        WorldRunConfig runConfig = WorldRunBalance.Create(worldConfigId, worldLevel, difficulty);

        ServerWorldSession.PendingSeed = seed;
        ServerWorldSession.SetPendingRunConfig(runConfig);

        if (sessions != null)
        {
            foreach (var onlineSession in sessions.GetOnlineSessions())
                onlineSession.SetPendingWorldQuestBootstrap(questIds, chainIds);

            ServerWorldSession.ResetPendingQuestBootstrap();
        }
        else
        {
            ServerWorldSession.PendingQuestIds = questIds ?? new List<string>();
            ServerWorldSession.PendingChainIds = chainIds ?? new List<string>();
        }

        ShowWorldLoadingObserversRpc(worldConfigId);

        Debug.Log(
            $"[PlayerSessionNetwork] Generated world seed {seed} for '{worldConfigId}' difficulty={runConfig.difficulty} level={runConfig.worldLevel}.");
        SceneTransitionService.LoadWorldScene();
    }

    [ObserversRpc]
    public void ShowHubLoadingObserversRpc()
    {
        LoadingScreenService.ShowHub("Returning players to hub...");
    }

    [ObserversRpc]
    public void ShowWorldLoadingObserversRpc(string worldConfigId)
    {
        LoadingScreenService.ShowWorld(worldConfigId, "Generating procedural world...");
    }

    [TargetRpc]
    private void TargetReceiveOnlinePlayers(
        NetworkConnection conn,
        string[] nicknames,
        string[] classIds,
        int[] levels)
    {
        var screen = Features.Multiplayer.UI.OnlinePlayersScreen.Resolve();
        if (screen == null)
            return;

        screen.ApplySnapshot(nicknames, classIds, levels);
    }

    [Server]
    public void ServerApplyRunCompletionExperience(
        int level,
        int experience,
        int gainedExperience)
    {
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
            stats.SetLevel(level);

        if (Owner != null)
            TargetApplyRunCompletionExperience(Owner, level, experience, gainedExperience);
    }

    [TargetRpc]
    private void TargetApplyRunCompletionExperience(
        NetworkConnection conn,
        int level,
        int experience,
        int gainedExperience)
    {
        var progress = PlayerProgressService.Instance;
        if (progress == null)
            return;

        var active = progress.GetActiveCharacter();
        int previousLevel = active != null ? active.level : level;

        progress.SetActiveCharacterProgress(level, experience);
        ApplyCampaignRunCompletion();

        if (level > previousLevel)
            Debug.Log($"[LEVEL] Run complete -> level {level} (+{gainedExperience} XP)");
        else
            Debug.Log(
                $"[LEVEL] Run complete -> +{gainedExperience} XP ({experience}/{PlayerProgressionRules.GetRequiredExperienceForLevel(level)})");
    }

    private static void ApplyCampaignRunCompletion()
    {
        CampaignProgressService progress = CampaignProgressService.I;
        CampaignRunContext run = CampaignRunContext.I;
        CampaignCatalogSO catalog = CampaignRuntimeState.CurrentCatalog;

        if (progress == null || run == null || !run.HasActiveRun)
            return;

        int threatCap = run.ShipThreatCap > 0
            ? run.ShipThreatCap
            : CampaignCatalogUtility.GetShipThreatCap(catalog, progress.ShipLevel);

        progress.CompleteBiomeThreat(
            run.PlanetId,
            run.BiomeId,
            run.ThreatLevel,
            threatCap);

        if (catalog != null)
        {
            PlanetConfig planet = CampaignCatalogUtility.FindPlanet(catalog, run.PlanetId);
            if (planet != null)
                progress.TryUnlockPlanetMission(planet);
        }

        run.Clear();
    }
}
