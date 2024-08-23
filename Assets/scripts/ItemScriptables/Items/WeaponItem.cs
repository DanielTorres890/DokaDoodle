using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Weapon")]
public class WeaponItem : ItemBase
{
    public void Awake()
    {
        type = ItemType.Equipment;
    }

    public override void ItemInfoCheck(int player, int itemId)
    {

    }
    public override void PerformItemEffect(int player, InventoryObject inventory)
    {
        
    }
}
