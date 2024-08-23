using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayInventory : MonoBehaviour
{
    
    public InventoryObject inventory;

    [SerializeField] private GameObject itemPrefab;
    public int X_Start;
    public int Y_Start;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;
    public Dictionary<InventorySlot,GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        CreateDisplay(0,NetworkData.Instance.currentPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < inventory.container.Count; i++) 
        {
           
            if (itemsDisplayed.ContainsKey(inventory.container[i]))
            {
                
            }
            else
            {
                var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.transform.GetComponent<Image>().sprite = inventory.container[i].item.itemSprite;
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

                obj.GetComponent<Button>().onClick.AddListener(delegate { inventory.container[i].item.ItemInfoCheck(NetworkData.Instance.currentPlayer, inventory.container[i].Id); });
                obj.GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].item.name;
                itemsDisplayed.Add(inventory.container[i], obj);
            }
        
        }
    }


    public void CreateDisplay(int inventoryType, int playerNum)
    {
        SetInventory(inventoryType, playerNum);

        for (int i = 0; i < inventory.container.Count; i++)
        {
            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.transform.GetComponent<Image>().sprite = inventory.container[i].item.itemSprite;
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
            
            obj.GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].item.name;

            

            itemsDisplayed.Add(inventory.container[i], obj);  
            
            
        }
    }

    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action )
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

    
    public Vector3 GetPosition(int i)
    {
        return new Vector3(X_Start +( X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)),Y_Start + (-Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }
}
