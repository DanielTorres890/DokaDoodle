using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Object", menuName = "Inventory System/Items/Equipment")]
public class EquipmentItem : ItemBase
{
    public void Awake()
    {
        type = ItemType.Equipment;
    }

    public override void PerformItemEffect(int player, ref InventoryObject inventory)
    {
        
    }
}
