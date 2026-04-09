using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQuestNetwork : NetworkBehaviour
{
    private PlayerQuestComponent questComponent;

    private void Awake()
    {
        questComponent = GetComponent<PlayerQuestComponent>();
    }

    [ServerRpc]
    public void GiveQuestsServerRpc(List<string> questIds)
    {
        questComponent?.GiveQuests(questIds);
    }

    [ServerRpc]
    public void GiveChainsServerRpc(List<string> chainIds)
    {
        questComponent?.GiveChains(chainIds);
    }

    [ServerRpc]
    public void ClearQuestsServerRpc()
    {
        questComponent?.ClearAll();
    }

    [ServerRpc]
    public void DebugAdvanceQuestServerRpc(string questId)
    {
        questComponent?.DebugAdvance(questId);
    }

    [ServerRpc]
    public void DebugCompleteQuestServerRpc(string questId)
    {
        questComponent?.DebugCompleteQuest(questId);
    }

    [ServerRpc]
    public void DebugFailQuestServerRpc(string questId)
    {
        questComponent?.DebugFailQuest(questId);
    }
}
