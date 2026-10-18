using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public RectTransform myTransform;
    public bool dragOnSurfaces = true;
    public Vector2 prevMousePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //var canvas = GetComponentInParent<Canvas>();
        //if (canvas == null) { return; }

        //if (dragOnSurfaces)
        //    myTransform = transform as RectTransform;
        //else
        //    myTransform = canvas.transform as RectTransform;

        //SetDraggedPosition(eventData);
        prevMousePos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }


    private void SetDraggedPosition(PointerEventData data)
    {
        //if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
        //    myTransform = data.pointerEnter.transform as RectTransform;

        Vector2 position = data.position - prevMousePos;
        myTransform.position += new Vector3(position.x, position.y);
        prevMousePos = data.position;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }
}
