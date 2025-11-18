using UnityEngine;
using System.Collections.Generic;

public class BuffHUD : MonoBehaviour
{
    public BuffSystem buffSystem;
    public Transform container;
    public BuffIconUI iconPrefab;

    private readonly Dictionary<BuffInstance, BuffIconUI> icons = new();

    private void Start()
    {
        if (buffSystem == null)
            buffSystem = FindAnyObjectByType<BuffSystem>();

        buffSystem.OnBuffAdded += AddIcon;
        buffSystem.OnBuffRemoved += RemoveIcon;
    }

    private void AddIcon(BuffInstance buff)
    {
        BuffIconUI ui = Instantiate(iconPrefab, container);
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
