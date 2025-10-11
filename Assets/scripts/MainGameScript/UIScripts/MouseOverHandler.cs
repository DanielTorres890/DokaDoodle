using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//this class is intended as a quick way to add mouseover events to anything (like tooltips or text that updates on mouse over)
public class MouseOverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onMouseEnter;
    public UnityEvent onMouseLeave;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseEnter.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseLeave.Invoke();
    }
}
