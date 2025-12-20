using UnityEngine;
using Features.Player;
using Features.Input;
using Features.Inventory.UnityIntegration;
using Features.Quests.UnityIntegration;

public sealed class InputHandlersSpawner : MonoBehaviour
{
    private void Start()
    {
        var ctx = GetComponent<PlayerInputContext>();
        if (ctx == null)
        {
            Debug.LogError(
                "[InputHandlersSpawner] PlayerInputContext missing",
                this);
            return;
        }

        Spawn<PauseInputHandler>(ctx);
        Spawn<UIBackInputHandler>(ctx);
        Spawn<InventoryInputHandler>(ctx);
        Spawn<QuestJournalInputHandler>(ctx);
        Spawn<InventoryUIInputController>(ctx);
    }

    private void Spawn<T>(PlayerInputContext ctx)
        where T : MonoBehaviour, IInputContextConsumer
    {
        var existing = GetComponent<T>();
        if (existing != null)
            return;

        var handler = gameObject.AddComponent<T>();
        handler.BindInput(ctx);
    }
}
