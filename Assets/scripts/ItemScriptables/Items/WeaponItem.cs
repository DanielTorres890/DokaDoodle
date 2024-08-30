using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Weapon")]
public class WeaponItem : ItemBase
{
    public AttackBase attack;
    public void Awake()
    {
        type = ItemType.Weapon;
    }

    public override void ItemInfoCheck(int player, int itemId)
    {

         if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        var tmp = 1;
       
        if ( this.type == ItemType.Magic)
        {
            tmp = 2;
        }
        ClientChecks.Instance.ConfirmBuffRpc(player, itemId, tmp);
    }
    public override void PerformItemEffect(int player, InventoryObject inventory)
    {

        if (NetworkData.Instance.players[player].equipItems[this.type] < 0)
        {

            NetworkData.Instance.players[player].equipItems[this.type] = inventory.database.GetId[this];
            foreach (var attrib in base.buffs)
            {
                NetworkData.Instance.players[player].stats[attrib.attribute] += attrib.value;
               
            }
            
            inventory.RemoveItem(this);
           
        }
        else
        {
    
            var temp = NetworkData.Instance.players[player].equipItems[this.type];
            NetworkData.Instance.players[player].equipItems[this.type] = inventory.database.GetId[this];
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

        if(this.type == ItemType.Weapon)
        {
            var tmp = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform.GetChild(0);
            tmp.gameObject.SetActive(true);
            tmp.GetComponent<SpriteRenderer>().sprite = this.itemSprite;
        }
        if(this.type == ItemType.Shield)
        {
            var tmp = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform.GetChild(1);
            tmp.gameObject.SetActive(true);
            tmp.GetComponent<SpriteRenderer>().sprite = this.itemSprite;
        }
        
    }
}
