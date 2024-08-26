using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Weapon")]
public class WeaponItem : ItemBase
{
    public void Awake()
    {
        type = ItemType.Weapon;
    }

    public override void ItemInfoCheck(int player, int itemId)
    {

         if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }
      
        ClientChecks.Instance.ConfirmBuffRpc(player, itemId, 1);
    }
    public override void PerformItemEffect(int player, InventoryObject inventory)
    {

        if (NetworkData.Instance.players[player].equipWeaponId < 0)
        {

            NetworkData.Instance.players[player].equipWeaponId = inventory.database.GetId[this];
            foreach (var attrib in base.buffs)
            {
                NetworkData.Instance.players[player].stats[attrib.attribute] += attrib.value;
               
            }
            Debug.Log(inventory.container.Count);
            inventory.RemoveItem(this);
            Debug.Log(inventory.container.Count);
        }
        else
        {
    
            var temp = NetworkData.Instance.players[player].equipWeaponId;
            NetworkData.Instance.players[player].equipWeaponId = inventory.database.GetId[this];
            inventory.AddItem(inventory.database.GetItem[temp]);
            foreach (var attrib in inventory.database.GetItem[temp].buffs)
            {
                NetworkData.Instance.players[player].stats[attrib.attribute] -= attrib.value;
                
            }
            foreach (var attrib in base.buffs)
            {
                NetworkData.Instance.players[player].stats[attrib.attribute] += attrib.value;
              
            }
            inventory.RemoveItem(this);
        }
        
    }
}
