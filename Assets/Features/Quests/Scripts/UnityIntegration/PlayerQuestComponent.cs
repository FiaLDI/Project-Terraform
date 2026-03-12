using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;
using Features.Quests.Domain;
using Features.Quests.Data;
using Features.Quests.Application;
using Features.Player.UI;

public class PlayerQuestComponent : NetworkBehaviour
{
    public readonly SyncDictionary<string, QuestNetState> Quests = new();

    private QuestService service;
    private QuestChainService chainService;

    [SerializeField] private QuestDatabaseAsset questDatabase;
    [SerializeField] private QuestChainDatabaseAsset chainDatabase;

    private readonly List<QuestNetState> pendingUpdates = new();
    private float batchTimer;

    private const float BATCH_INTERVAL = 0.2f;

    public override void OnStartServer()
    {
        service = new QuestService();
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
            quest.GetTotalProgress(),
            quest.GetTotalTarget(),
            false
        );

        pendingUpdates.Add(state);
    }

    private void OnQuestUpdated(QuestRuntime quest)
    {
        var id = quest.Definition.Id.Value;

        var state = new QuestNetState(
            id,
            quest.GetTotalProgress(),
            quest.GetTotalTarget(),
            quest.State == QuestState.Completed
        );

        pendingUpdates.Add(state);
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
        if (source != gameObject)
            return;

        service.HandleEvent(e);
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
}
