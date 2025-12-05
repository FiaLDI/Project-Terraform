using UnityEngine;
using UnityEngine.UI;
using Features.Classes.Data;

public class ClassSelectionUI : MonoBehaviour
{
    public PlayerClassLibrarySO library;
    public Transform container;
    public ClassSelectionButton buttonPrefab;
    public PlayerClassController player;

    private void Start()
    {
        foreach (var cfg in library.classes)
        {
            var btn = Instantiate(buttonPrefab, container);
            btn.Set(cfg);
            btn.onClick += () =>
            {
                player.ApplyClass(cfg);
            };
        }
    }
}
