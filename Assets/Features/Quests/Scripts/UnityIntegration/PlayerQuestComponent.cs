using FishNet.Object;
using Features.Quests.Domain;
using Features.Quests.Data;
using Features.Quests.Application;
using UnityEngine;

public class PlayerQuestComponent : NetworkBehaviour
{
    public QuestService Service { get; private set; }

    private QuestChainService chainService;

    [SerializeField] private QuestDatabaseAsset questDatabase;
    [SerializeField] private QuestChainDatabaseAsset chainDatabase;

    public override void OnStartServer()
    {
        Service = new QuestService();
        chainService = new QuestChainService(Service);

        SubscribeEvents();
        GiveInitialQuests();
    }

    public override void OnStopServer()
    {
        UnsubscribeEvents();
    }

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

    private void HandleEvent(object source, IQuestEvent e)
    {
        if (source != gameObject)
            return;

        Service.HandleEvent(e);
    }

    private void GiveInitialQuests()
    {
        foreach (var id in ServerWorldSession.PendingQuestIds)
        {
            var def = questDatabase.GetDefinition(id);

            if (def != null)
                Service.StartQuest(def);
        }

        foreach (var id in ServerWorldSession.PendingChainIds)
        {
            var chain = chainDatabase.GetDefinition(id);

            if (chain != null)
                chainService.StartChain(chain);
        }
    }
}
