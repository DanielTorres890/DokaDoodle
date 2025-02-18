using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory",menuName = "Inventory System/Inventory" )]

public class InventoryObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemDataBase database;
    public List<InventorySlot> container = new List<InventorySlot>();
    public int MAXSIZE;

    public bool AddItem(ItemBase _item)
    {
        bool success = false;
        if (container.Count < MAXSIZE  )
        {
            success = true;
            container.Add(new InventorySlot(database.GetId[_item], _item));
        }
        else
        {
            Debug.Log(" U DONT HAVE SPACE FOR THAT ");
        }
        return success;

    }
    public void RemoveItem(ItemBase _item)
    {
        for (int i = container.Count - 1; i >= 0; i--)
        {
            Debug.Log(container[i].item.itemName);
            Debug.Log(_item.itemName);
            if (container[i].item == _item)
            {
                Debug.Log("Removed!");
                container.RemoveAt(i);

                return;
            }
        }

       // List<InventorySlot> slotsToRemove = new List<InventorySlot>();
       
        /* foreach (InventorySlot slot in container)
        {
            if (slot.item == _item)
            {
                slotsToRemove.Add(slot);
            }
        }
        foreach (InventorySlot slot in slotsToRemove)
        {
            container.Remove(slot);
        } */
    }
    public void RemoveItem(int index)
    {
        container.RemoveAt(index);
    }
    public ItemBase getItem(int index)
    {
        return container[index].item;
    }
    public void ToFront(ItemBase _item)
    {
        for (int i = container.Count - 1; i >= 0; i--)
        {
            if (container[i].item == _item)
            {
                container.Insert(0, container[i]);
                container.RemoveAt(i+1);
                return;
            }

        }
    }
    public void OnAfterDeserialize()
    {
        for (int i  = 0; i < container.Count; i++)
        {
            container[i].item = database.GetItem[container[i].Id];

        }
    }

    public void OnBeforeSerialize()
    {
        
    }
    public List<int> serializeInventory ()
    {
        List<int> result = new List<int>();
        for(int i = 0;i < container.Count;i++)
        {
            result.Add(container[i].Id);
        }
        return result;
    }
}

[System.Serializable]
public class InventorySlot
{
    public int Id;
    public ItemBase item;

    public InventorySlot(int id, ItemBase item)
    {
        Id = id;
        this.item = item;
    }
}