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
        if (transform.parent.name.Equals("CommandArea"))
        {
            myTransform.position = new Vector3(PlayerPrefs.GetFloat("CommandAreaX", myTransform.position.x), PlayerPrefs.GetFloat("CommandAreaY", myTransform.position.y), PlayerPrefs.GetFloat("CommandAreaZ", myTransform.position.z));
            
           
        }
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
        if(transform.parent.name.Equals("CommandArea"))
        {
            PlayerPrefs.SetFloat("CommandAreaX", myTransform.position.x);
            PlayerPrefs.SetFloat("CommandAreaY", myTransform.position.y);
            PlayerPrefs.SetFloat("CommandAreaZ", myTransform.position.z);
        }
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
