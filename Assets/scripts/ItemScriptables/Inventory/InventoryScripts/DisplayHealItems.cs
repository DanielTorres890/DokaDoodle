using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayHealItems : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public InventoryObject inventory;

    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TextMeshProUGUI displayText;
    public Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();
    public Transform parent;
    public AllyViewNetwork allyView;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void UpdateDisplay()
    {
        for (int i = 0; i < inventory.container.Count; i++)
        {
            if (inventory.container[i].item is not FoodItem) { continue; }

            FoodItem potentialItem = inventory.container[i].item as FoodItem;
            if (potentialItem.buffs[0].attribute != Attributes.Health) { continue; }

            var tempId = i;
            if (itemsDisplayed.ContainsKey(inventory.container[i]))
            {

            }
            else
            {
                var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.transform.GetComponent<Image>().sprite = inventory.container[i].item.itemSprite;
                obj.transform.SetParent(parent);
                obj.transform.SetAsFirstSibling();


                obj.GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].item.name;
                AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(inventory.container[tempId].item.description); });
                AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(inventory.container[tempId].item.description); });
                itemsDisplayed.Add(inventory.container[i], obj);
            }

        }
    }


    public void CreateDisplay(int playerNum, int inventoryType = 0)
    {
        SetInventory(inventoryType, playerNum);

        for (int i = 0; i < inventory.container.Count; i++)
        {
            if (inventory.container[i].item is not FoodItem) { continue; }

            FoodItem potentialItem = inventory.container[i].item as FoodItem;
            if (potentialItem.buffs[0].attribute != Attributes.Health) { continue; }

            var tempId = i; //WHY IS THIS A THING THAT HAS TO BE DONE
            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.transform.SetAsFirstSibling();
            obj.transform.GetComponent<Image>().sprite = inventory.container[i].item.itemSprite;
         


            if (i >= inventory.MAXSIZE)
            {
                var panel = obj.transform.GetChild(0).GetComponent<Image>();
                panel.color = new Color(panel.color.a, panel.color.g, panel.color.b, 0.5f);
            }
            //u know im not happy about this but lowkey it just seems easier to reuse this ngl
            obj.GetComponent<Button>().onClick.AddListener( delegate { allyView.HealAlly(tempId); } );
            obj.GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].item.name;
            obj.transform.SetParent(parent);
            AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(inventory.container[tempId].item.description); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(inventory.container[tempId].item.description); });

            //UnityAction<GameObject> action = new UnityAction<GameObject>(delegate { inventory.container[tempId].item.ItemInfoCheck(NetworkData.Instance.currentPlayer, inventory.container[tempId].Id); });
            //UnityEventTools.AddObjectPersistentListener<GameObject>(obj.GetComponent<Button>().onClick, action, obj);




            itemsDisplayed.Add(inventory.container[i], obj);


        }
    }

    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);

    }
    private void SetInventory(int inventoryType, int playerNum)
    {



        inventory = NetworkData.Instance.playerInventories[playerNum][inventoryType];



        foreach (GameObject item in itemsDisplayed.Values)
        {
            Destroy(item);
        }
        itemsDisplayed.Clear();
    }


   
}
