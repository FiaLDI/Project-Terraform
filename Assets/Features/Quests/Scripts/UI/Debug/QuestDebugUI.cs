using UnityEngine;
using Features.UI;
using Features.Input;
using Features.Quests.Data;
using UnityEngine.UI;
using System.Linq;

public class QuestDebugUI : PlayerBoundUIView, IUIScreen
{
    [SerializeField] private GameObject root;
    [SerializeField] private QuestDebugListUI list;

    [Header("Buttons")]
    [SerializeField] private Button giveAllQuestsBtn;
    [SerializeField] private Button giveAllChainsBtn;
    [SerializeField] private Button clearBtn;

    [Header("Data")]
    [SerializeField] private QuestAsset[] quests;
    [SerializeField] private QuestChainAsset[] chains;

    private PlayerNetworkController net;

    public InputMode Mode => InputMode.Dialog;

    protected override void OnPlayerBound(GameObject player)
    {
        root.SetActive(false);

        var quest = player.GetComponent<PlayerQuestComponent>();
        net   = player.GetComponent<PlayerNetworkController>();

        list.Init(quest, net);

        BindButtons();
    }

    private void BindButtons()
    {
        giveAllQuestsBtn.onClick.RemoveAllListeners();
        giveAllChainsBtn.onClick.RemoveAllListeners();
        clearBtn.onClick.RemoveAllListeners();

        giveAllQuestsBtn.onClick.AddListener(OnGiveAllQuests);
        giveAllChainsBtn.onClick.AddListener(OnGiveAllChains);
        clearBtn.onClick.AddListener(OnClear);
    }

    private void OnGiveAllQuests()
    {
        if (net == null) return;

        var ids = quests
            .Where(q => q != null)
            .Select(q => q.questId)
            .ToList();

        net.GiveQuestsServerRpc(ids);
    }

    private void OnGiveAllChains()
    {
        if (net == null) return;

        var ids = chains
            .Where(c => c != null)
            .Select(c => c.chainId)
            .ToList();

        net.GiveChainsServerRpc(ids);
    }

    private void OnClear()
    {
        net?.ClearQuestsServerRpc();
    }

    public void Show()
    {
        root.SetActive(true);
        InputModeManager.I.SetMode(Mode);
    }

    public void Hide()
    {
        root.SetActive(false);
        InputModeManager.I.SetMode(InputMode.Gameplay);
    }

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    public void OnCloseClicked()
    {
        UIStackManager.I.Pop();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);
    }

    protected override void OnDisable()
    {
        UIRegistry.I?.Unregister(this);
        base.OnDisable();
    }
}
