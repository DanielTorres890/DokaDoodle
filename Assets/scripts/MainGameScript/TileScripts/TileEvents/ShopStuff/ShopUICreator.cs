using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUICreator : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    public int X_Start;
    public int Y_Start;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    [SerializeField] private ItemBase[] displayedItems;
    private List<GameObject> displayedGameObjects = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI displayText;


    private void Awake()
    {
        //if im being fr this is bc my brain does NOT feel like making shop buying things function similarly to player inventory icl frfr fmcl 
        displayedItems = (NetworkData.Instance.currentEvent as ShopEvent).itemsSold;
        CreateDisplay();
    }

    public void CreateDisplay()
    {
       

        for (int i = 0; i < displayedItems.Length; i++)
        {
            var tempId = i; //WHY IS THIS A THING THAT HAS TO BE DONE
            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.transform.GetComponent<Image>().sprite = displayedItems[i].itemSprite;
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

            obj.GetComponent<Button>().onClick.AddListener(delegate { ShopUISync.instance.setUpBuy(tempId); });
            AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(displayedItems[tempId].description); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(displayedItems[tempId].description); });

            //UnityAction<GameObject> action = new UnityAction<GameObject>(delegate { inventory.container[tempId].item.ItemInfoCheck(NetworkData.Instance.currentPlayer, inventory.container[tempId].Id); });
            //UnityEventTools.AddObjectPersistentListener<GameObject>(obj.GetComponent<Button>().onClick, action, obj);
            var temp = obj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (Mathf.RoundToInt(displayedItems[i].itemValue * NetworkData.Instance.globalShopMultiplier) <= NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo[PlayerInfo.money])
            {
             
                temp.text = string.Format("{0, -13} {1}", displayedItems[i].name, Mathf.RoundToInt(displayedItems[i].itemValue * NetworkData.Instance.globalShopMultiplier));
            }

            else
            {
                temp.overrideColorTags = true;
                temp.text = string.Format("<color=red>{0, -13} {1} </color>", displayedItems[i].name, Mathf.RoundToInt(displayedItems[i].itemValue * NetworkData.Instance.globalShopMultiplier));
                temp.color = Color.red;

            }
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
