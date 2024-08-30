using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Object", menuName = "Inventory System/Items/Food")]
public class FoodItem : ItemBase
{
   
    public void Awake()
    {
        type = ItemType.Food;
    }

    public override void ItemInfoCheck(int player, int itemId)
    {
        if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        ClientChecks.Instance.ConfirmBuffRpc(player, itemId, 0);
    }

    public override void PerformItemEffect(int player, InventoryObject inventory)
    {
        
        foreach (var attrib in base.buffs)
        {
            if (attrib.attribute == Attributes.Health && NetworkData.Instance.players[player].stats[attrib.attribute] + attrib.value > NetworkData.Instance.players[player].stats[Attributes.MaxHealth])
            {
                NetworkData.Instance.players[player].stats[attrib.attribute] = NetworkData.Instance.players[player].stats[Attributes.MaxHealth];
            }
            else { NetworkData.Instance.players[player].stats[attrib.attribute] += attrib.value;  }
            
            
        }


        inventory.RemoveItem(this);
        
    }
}
