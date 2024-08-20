using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory",menuName = "Inventory System/Inventory" )]

public class InventoryObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemDataBase database;
    public List<InventorySlot> container = new List<InventorySlot>();
    public void AddItem(ItemBase _item)
    {
        container.Add(new InventorySlot(database.GetId[_item], _item));

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