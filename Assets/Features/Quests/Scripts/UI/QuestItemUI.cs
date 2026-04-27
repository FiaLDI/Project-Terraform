using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class QuestItemUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Conditions;

    private Action clickHandler;

    public void SetClickHandler(Action handler)
    {
        clickHandler = handler;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickHandler?.Invoke();
    }
}
