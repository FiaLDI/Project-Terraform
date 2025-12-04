using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;

[DefaultExecutionOrder(-300)]
public class StatsRoot : MonoBehaviour
{
    public IStatsFacade Stats { get; private set; }
    public StatsFacadeAdapter Adapter { get; private set; }

    private void Awake()
    {
        // создаём ДОМЕННЫЕ СТАТЫ ОДИН РАЗ
        Stats = new StatsFacade();

        // создаём адаптер фасада ТОЛЬКО ОДИН РАЗ
        Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
        Adapter.Init(Stats);
    }
}
