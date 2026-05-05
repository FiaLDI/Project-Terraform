using System.Collections.Generic;
using System.Linq;
using Features.Inventory.Domain;
using Features.Player.UI;
using Features.Quests.Application;
using Features.Quests.Data;
using Features.Quests.Domain;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Multiplayer.Domain;
using UnityEngine;

public class PlayerQuestComponent : NetworkBehaviour
{
    public readonly SyncDictionary<string, QuestNetState> Quests = new();
    private readonly HashSet<string> rewarded = new();
    private readonly HashSet<string> advancedChains = new();

    private QuestService service;
    private QuestChainService chainService;

    [SerializeField] private QuestDatabaseAsset questDatabase;
    [SerializeField] private QuestChainDatabaseAsset chainDatabase;

    private readonly List<QuestNetState> pendingUpdates = new();
    private float batchTimer;

    private const float BATCH_INTERVAL = 0.2f;

    public override void OnStartServer()
    {
        service = new QuestService(gameObject);
        chainService = new QuestChainService(service);

        RestoreOrBootstrapQuests(ResolveSession());

        service.OnQuestAdded += OnQuestAdded;
        service.OnQuestUpdated += OnQuestUpdated;
        service.OnQuestRemoved += OnQuestRemoved;

        SubscribeEvents();
        EnqueueAllQuestStates();
        FlushBatch();
    }

    public override void OnStopServer()
    {
        SaveQuestDataToSession();
        UnsubscribeEvents();

        if (service != null)
        {
            service.OnQuestAdded -= OnQuestAdded;
            service.OnQuestUpdated -= OnQuestUpdated;
            service.OnQuestRemoved -= OnQuestRemoved;
        }
    }

    private void Update()
    {
        if (!IsServer)
            return;

        batchTimer += Time.deltaTime;

        if (batchTimer >= BATCH_INTERVAL)
        {
            FlushBatch();
            batchTimer = 0f;
        }
    }

    private void FlushBatch()
    {
        foreach (var q in pendingUpdates)
        {
            Quests[q.questId] = q;
        }

        pendingUpdates.Clear();
    }

    // =============================
    // QuestService events
    // =============================

    private void OnQuestAdded(QuestRuntime quest)
    {
        QueueQuestState(quest);
        SaveQuestDataToSession();
    }

    private void OnQuestUpdated(QuestRuntime quest)
    {
        TryAdvanceChain(quest);
        TryGiveRewards(quest);
        QueueQuestState(quest);
        SaveQuestDataToSession();
    }

    private void OnQuestRemoved(QuestRuntime quest)
    {
        Quests.Remove(quest.Definition.Id.Value);
        SaveQuestDataToSession();
    }

    // =============================
    // QuestEventBus
    // =============================

    private void SubscribeEvents()
    {
        QuestEventBus.Subscribe<EnemyKilledEvent>(HandleEvent);
        QuestEventBus.Subscribe<ItemAddedEvent>(HandleEvent);
        QuestEventBus.Subscribe<PointReachedEvent>(HandleEvent);
        QuestEventBus.Subscribe<InteractionEvent>(HandleEvent);
        QuestEventBus.Subscribe<TickEvent>(HandleEvent);
    }

    private void UnsubscribeEvents()
    {
        QuestEventBus.Unsubscribe<EnemyKilledEvent>(HandleEvent);
        QuestEventBus.Unsubscribe<ItemAddedEvent>(HandleEvent);
        QuestEventBus.Unsubscribe<PointReachedEvent>(HandleEvent);
        QuestEventBus.Unsubscribe<InteractionEvent>(HandleEvent);
        QuestEventBus.Unsubscribe<TickEvent>(HandleEvent);
    }

    private void HandleEvent<T>(object source, T e)
    where T : IQuestEvent
    {
        if (!IsServer)
            return;

        bool isMine = e.Source == gameObject;

        service.HandleEventFiltered(e, isMine);
    }

    private QuestConditionNetState[] BuildConditions(QuestRuntime quest)
    {
        var list = new List<QuestConditionNetState>();

        foreach (var cond in quest.Definition.Conditions)
        {
            list.Add(new QuestConditionNetState(
                quest.GetProgress(cond),
                quest.GetTarget(cond)
            ));
        }

        return list.ToArray();
    }

    private PlayerUIRoot GetLocalPlayer()
    {
        return GetComponentInParent<PlayerUIRoot>();
    }

    private QuestNetState BuildNetState(QuestRuntime quest)
    {
        return new QuestNetState(
            quest.Definition.Id.Value,
            BuildConditions(quest),
            quest.State
        );
    }

    private void QueueQuestState(QuestRuntime quest)
    {
        if (quest == null)
            return;

        pendingUpdates.Add(BuildNetState(quest));
    }

    private void EnqueueAllQuestStates()
    {
        pendingUpdates.Clear();

        if (service == null)
            return;

        foreach (var quest in service.ActiveQuests)
            QueueQuestState(quest);
    }

    // =============================
    // Initial quests
    // =============================

    private void GiveInitialQuests(IEnumerable<string> questIds, IEnumerable<string> chainIds)
    {
        foreach (var id in questIds ?? Enumerable.Empty<string>())
        {
            var def = questDatabase.GetDefinition(id);

            if (def != null)
                service.StartQuest(def);
        }

        foreach (var id in chainIds ?? Enumerable.Empty<string>())
        {
            var chain = chainDatabase.GetDefinition(id);

            if (chain != null)
                chainService.StartChain(chain);
        }
    }

    private bool TryConsumeBootstrapSelection(
        PlayerSession session,
        out List<string> questIds,
        out List<string> chainIds)
    {
        if (session != null && session.HasPendingWorldQuestBootstrap)
        {
            (questIds, chainIds) = session.ConsumePendingWorldQuestBootstrap();
            return true;
        }

        if (ServerWorldSession.PendingQuestIds.Count > 0 || ServerWorldSession.PendingChainIds.Count > 0)
        {
            (questIds, chainIds) = ServerWorldSession.ConsumeQuestBootstrap();
            return true;
        }

        questIds = null;
        chainIds = null;
        return false;
    }

    private void RestoreOrBootstrapQuests(PlayerSession session)
    {
        rewarded.Clear();
        advancedChains.Clear();
        chainService?.Clear();

        if (TryConsumeBootstrapSelection(session, out var questIds, out var chainIds))
        {
            GiveInitialQuests(questIds, chainIds);
            SaveQuestDataToSession(session);
            return;
        }

        if (session != null && session.HasQuestData)
        {
            RestoreFromSession(session.QuestData);
            return;
        }

        GiveInitialQuests(Enumerable.Empty<string>(), Enumerable.Empty<string>());
        SaveQuestDataToSession(session);
    }

    private void RestoreFromSession(QuestPersistenceState persisted)
    {
        if (persisted == null)
            return;

        foreach (var id in persisted.RewardedQuestIds)
            rewarded.Add(id);

        foreach (var id in persisted.AdvancedQuestIds)
            advancedChains.Add(id);

        foreach (var snapshot in persisted.Quests)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.QuestId))
                continue;

            var def = questDatabase.GetDefinition(snapshot.QuestId);
            if (def == null)
                continue;

            service.RestoreQuest(def, snapshot.Conditions, snapshot.State);
        }

        foreach (var snapshot in persisted.Chains)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.ChainId))
                continue;

            var def = chainDatabase.GetDefinition(snapshot.ChainId);
            if (def == null)
                continue;

            chainService.RestoreChain(def, snapshot.Index);
        }
    }

    private PlayerSession ResolveSession()
    {
        if (!IsServer || Owner == null)
            return null;

        var root = ServerCompositionRoot.I;
        return root?.Sessions?.GetSessionByClient(Owner.ClientId);
    }

    private void SaveQuestDataToSession(PlayerSession explicitSession = null)
    {
        if (!IsServer || service == null || chainService == null)
            return;

        var session = explicitSession ?? ResolveSession();
        if (session == null)
            return;

        var persisted = new QuestPersistenceState
        {
            Initialized = true
        };

        foreach (var quest in service.ActiveQuests)
        {
            persisted.Quests.Add(new QuestStateSnapshot(
                quest.Definition.Id.Value,
                BuildConditions(quest),
                quest.State));
        }

        foreach (var chain in chainService.GetSnapshots())
            persisted.Chains.Add(chain);

        foreach (var id in rewarded)
            persisted.RewardedQuestIds.Add(id);

        foreach (var id in advancedChains)
            persisted.AdvancedQuestIds.Add(id);

        session.SetQuestData(persisted);
    }

    private void TryGiveRewards(QuestRuntime quest)
    {
        if (quest.State != QuestState.Completed)
            return;

        var id = quest.Definition.Id.Value;

        if (rewarded.Contains(id))
            return;

        rewarded.Add(id);

        GiveRewards(quest);
    }

    private void TryAdvanceChain(QuestRuntime quest)
    {
        if (quest.State != QuestState.Completed)
            return;

        var id = quest.Definition.Id.Value;

        if (advancedChains.Contains(id))
            return;

        advancedChains.Add(id);
        chainService?.Advance(quest.Definition.Id);
    }

    private void GiveRewards(QuestRuntime quest)
    {
        var net = GetComponent<InventoryStateNetwork>();
        if (net == null)
            return;

        foreach (var reward in quest.Definition.Rewards)
        {
            net.ExecuteCommandServer(new InventoryCommandData
            {
                Command = InventoryCommand.GiveReward,
                RewardItemId = reward.ItemId,
                RewardAmount = reward.Amount,
                RewardLevel = 0
            });
        }
    }

    [Server]
    public bool AreAllStartedQuestsCompleted()
    {
        if (service == null)
            return false;

        foreach (var quest in service.ActiveQuests)
        {
            if (quest == null)
                continue;

            if (quest.State != QuestState.Completed)
                return false;
        }

        return true;
    }

    [Server]
    public bool HasQuest(string questId)
    {
        if (service == null || string.IsNullOrWhiteSpace(questId))
            return false;

        return service.TryGetQuest(new QuestId(questId), out _);
    }

    [Server]
    public bool IsQuestCompleted(string questId)
    {
        if (service == null || string.IsNullOrWhiteSpace(questId))
            return false;

        if (!service.TryGetQuest(new QuestId(questId), out QuestRuntime quest))
            return false;

        return quest.State == QuestState.Completed;
    }

    [Server]
    public void GiveQuests(List<string> questIds)
    {
        foreach (var id in questIds)
        {
            var def = questDatabase.GetDefinition(id);
            if (def != null)
                service.StartQuest(def);
        }
    }

    [Server]
    public void GiveChains(List<string> chainIds)
    {
        foreach (var id in chainIds)
        {
            var def = chainDatabase.GetDefinition(id);
            if (def != null)
                chainService.StartChain(def);
        }
    }

    [Server]
    public void ClearAll()
    {
        rewarded.Clear();
        advancedChains.Clear();
        pendingUpdates.Clear();
        chainService?.Clear();

        var active = service.ActiveQuests.ToList();

        foreach (var quest in active)
        {
            service.ResetQuest(quest.Definition.Id);
        }

        Quests.Clear();
        SaveQuestDataToSession();
    }

    [Server]
    public void DebugCompleteQuest(string questId)
    {
        service?.CompleteQuest(new QuestId(questId));
    }

    [Server]
    public void DebugFailQuest(string questId)
    {
        service?.FailQuest(new QuestId(questId));
    }

    [Server]
    public void DebugAdvance(string questId, int amount = 1)
    {
        var id = new QuestId(questId);

        if (!service.TryGetQuest(id, out var quest))
            return;

        QuestEventBus.Publish(
            new DebugProgressEvent(
                gameObject,
                id,
                amount
            )
        );
    }
}
