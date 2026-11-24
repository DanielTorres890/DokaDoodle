using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvSlotsDisplay : MonoBehaviour
{
    public InventoryObject inventory;

    [SerializeField] private GameObject itemPrefab;
    
    public List<GameObject> itemsDisplayed = new List<GameObject>();
    


    public void UpdateDisplay()
    {
        for (int i = 0; i < inventory.MAXSIZE; i++)
        {

                var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.transform.SetAsFirstSibling();
                itemsDisplayed.Add(obj);
            

        }
    }


    public void CreateDisplay(int playerNum, int inventoryType = 0)
    {
        SetInventory(inventoryType, playerNum);

        for (int i = 0; i < inventory.MAXSIZE; i++)
        {

            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.transform.SetAsFirstSibling();

            //UnityAction<GameObject> action = new UnityAction<GameObject>(delegate { inventory.container[tempId].item.ItemInfoCheck(NetworkData.Instance.currentPlayer, inventory.container[tempId].Id); });
            //UnityEventTools.AddObjectPersistentListener<GameObject>(obj.GetComponent<Button>().onClick, action, obj);




            itemsDisplayed.Add(obj);


        }
    }

    private void SetInventory(int inventoryType, int playerNum)
    {



        inventory = NetworkData.Instance.playerInventories[playerNum][inventoryType];
        foreach (GameObject item in itemsDisplayed)
        {
            Destroy(item);
        }
        itemsDisplayed.Clear();
    }

    
}
