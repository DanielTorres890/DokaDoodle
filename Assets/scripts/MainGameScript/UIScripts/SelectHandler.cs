using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SelectHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{

    public UnityEvent onSelect;
    public UnityEvent onDeselect;
    
    public void OnDeselect(BaseEventData eventData)
    {
        onDeselect.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect.Invoke();
    }

   
}
