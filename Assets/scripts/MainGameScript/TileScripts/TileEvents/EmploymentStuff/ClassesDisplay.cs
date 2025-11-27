using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClassesDisplay : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    public int X_Start;
    public int Y_Start;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    private List<GameObject> displayedGameObjects = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI displayText;


    private void Start()
    {
        //if im being fr this is bc my brain does NOT feel like making shop buying things function similarly to player inventory icl frfr fmcl 

        CreateDisplay();
    }

    public void CreateDisplay()
    {

        var classDictionary = NetworkData.Instance.GetCurrentPlayer().playerClassProgress;
        foreach (var key in classDictionary.Keys)
        {
            var tempId = key; //WHY IS THIS A THING THAT HAS TO BE DONE
            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(key);

            //COME BACK HERE LATER DOUCHEBAG
            obj.GetComponent<Button>().onClick.AddListener(delegate { EmploymentEventManager.instance.ChangePlayerClass(tempId); });

            AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[tempId].classDescription); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[tempId].classDescription); });

            //UnityAction<GameObject> action = new UnityAction<GameObject>(delegate { inventory.container[tempId].item.ItemInfoCheck(NetworkData.Instance.currentPlayer, inventory.container[tempId].Id); });
            //UnityEventTools.AddObjectPersistentListener<GameObject>(obj.GetComponent<Button>().onClick, action, obj);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = NetworkData.Instance.classDataBase.GetItem[tempId].className;
            displayedGameObjects.Add(obj);

        }
    }
    public void UpdateDisplay()
    {
        foreach(var obj in displayedGameObjects)
        {
            Destroy(obj);
        }
        displayedGameObjects.Clear();
        CreateDisplay();
    }
    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);

    }
    public Vector3 GetPosition(int i)
    {
        return new Vector3(X_Start + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_Start + (-Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }
}
