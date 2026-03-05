using UnityEngine;
using System.Collections.Generic;

public class PolygonGlowButtonGroup : MonoBehaviour
{
    public List<PolygonGlowButton> buttons = new();
    private PolygonGlowButton selectedButton;

    private void Awake()
    {
        foreach (var btn in buttons)
            btn.SetGroup(this);
    }

    public void OnButtonClicked(PolygonGlowButton btn)
    {
        if (btn.IsLocked) return;

        if (selectedButton != null && selectedButton != btn)
            selectedButton.SetState(ButtonState.Idle);

        selectedButton = btn;
        selectedButton.SetState(ButtonState.Selected);
    }

    public void LockButton(PolygonGlowButton btn)
    {
        btn.SetLocked(true);
    }

    public void UnlockButton(PolygonGlowButton btn)
    {
        btn.SetLocked(false);
    }
}
