using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsTabbedUI : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string tabName;
        public Button button;                  // или PolygonGlowButton
        public GameObject content;             // контент вкладки
    }

    public List<Tab> tabs = new List<Tab>();

    private int currentTab = 0;

    private void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => ShowTab(index));
        }

        ShowTab(0);
    }

    public void ShowTab(int index)
    {
        currentTab = index;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool active = (i == index);
            tabs[i].content.SetActive(active);

            if (tabs[i].button.TryGetComponent<PolygonGlowButton>(out var glow))
            {
                glow.SetState(active ? ButtonState.Selected : ButtonState.Idle);
            }
        }
    }
}
