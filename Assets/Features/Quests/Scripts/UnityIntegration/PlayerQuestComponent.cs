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

        service.OnQuestAdded += OnQuestAdded;
        service.OnQuestUpdated += OnQuestUpdated;
        service.OnQuestRemoved += OnQuestRemoved;

        SubscribeEvents();
        GiveInitialQuests();
    }

    public override void OnStopServer()
    {
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
        var id = quest.Definition.Id.Value;

        var state = new QuestNetState(
            id,
            BuildConditions(quest),
            quest.State == QuestState.Completed
        );

        pendingUpdates.Add(state);
    }

    private void OnQuestUpdated(QuestRuntime quest)
    {
        var id = quest.Definition.Id.Value;

        var state = new QuestNetState(
            id,
            BuildConditions(quest),
            quest.State == QuestState.Completed
        );

        pendingUpdates.Add(state);

        TryAdvanceChain(quest);
        TryGiveRewards(quest);
    }

    private void OnQuestRemoved(QuestRuntime quest)
    {
        Quests.Remove(quest.Definition.Id.Value);
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

    // =============================
    // Initial quests
    // =============================

    private void GiveInitialQuests()
    {
        foreach (var id in ServerWorldSession.PendingQuestIds)
        {
            var def = questDatabase.GetDefinition(id);

            if (def != null)
                service.StartQuest(def);
        }

        foreach (var id in ServerWorldSession.PendingChainIds)
        {
            var chain = chainDatabase.GetDefinition(id);

            if (chain != null)
                chainService.StartChain(chain);
        }
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
        TargetLevelUp(Owner);
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

    [TargetRpc]
    private void TargetLevelUp(NetworkConnection conn)
    {
        var progress = PlayerProgressService.Instance;
        if (progress == null)
            return;

        var character = progress.GetActiveCharacter();
        if (character == null)
            return;

        character.level += 1;

        Debug.Log($"[LEVEL] Level UP → {character.level}");

        progress.Save();
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
        var active = service.ActiveQuests.ToList();

        foreach (var quest in active)
        {
            service.ResetQuest(quest.Definition.Id);
        }
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
