using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTester : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData e)
    {
        Debug.Log("ENTER ON " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData e)
    {
        Debug.Log("EXIT ON " + gameObject.name);
    }
}
