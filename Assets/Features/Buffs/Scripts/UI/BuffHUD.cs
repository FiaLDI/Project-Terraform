using UnityEngine;
using System.Collections.Generic;

public class BuffHUD : MonoBehaviour
{
    public Transform container;
    public BuffIconUI iconPrefab;

    private BuffSystem playerBuffSystem;

    private readonly Dictionary<BuffInstance, BuffIconUI> icons = new();

    private void Start()
    {
        // пытаемся найти сразу
        TryBindToPlayer();
    }

    private void Update()
    {
        // если игрок появился позже (сетевой spawn), HUD привяжется
        if (playerBuffSystem == null)
            TryBindToPlayer();
    }

    private void TryBindToPlayer()
    {
        // Ищем игрока по компоненту PlayerEnergy
        var playerEnergy = FindFirstObjectByType<PlayerEnergy>();
        if (playerEnergy == null)
            return;

        playerBuffSystem = playerEnergy.GetComponent<BuffSystem>();
        if (playerBuffSystem == null)
            return;

        // подписываемся один раз
        playerBuffSystem.OnBuffAdded += AddIcon;
        playerBuffSystem.OnBuffRemoved += RemoveIcon;

        Debug.Log("[BuffHUD] Successfully bound to player BuffSystem");
    }

    private void AddIcon(BuffInstance buff)
    {
        var ui = Instantiate(iconPrefab, container);
        ui.Bind(buff);
        icons[buff] = ui;
    }

    private void RemoveIcon(BuffInstance buff)
    {
        if (icons.TryGetValue(buff, out var ui))
        {
            Destroy(ui.gameObject);
            icons.Remove(buff);
        }
    }
}
