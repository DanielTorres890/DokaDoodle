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

        ClientChecks.Instance.ShowConfirmItemButtonsRpc(player, itemId, 0);
    }

    public override void PerformItemEffect(int player, InventoryObject inventory)
    {
        
        foreach (var attrib in base.buffs)
        {
            if (attrib.attribute == Attributes.Health)
            {
                NetworkData.Instance.players[player].healHp(attrib.value);
                foreach(var ally in NetworkData.Instance.players[player].partyMembers)
                {
                    ally.healHp(attrib.value);
                }
                
            }
            else { NetworkData.Instance.players[player].stats[attrib.attribute] += attrib.value;  }
            
            
        }

        base.PerformItemEffect(player, inventory);
        
        
    }
    public int HealingAmount()
    {
        foreach(var attrib in buffs)
        {
            if(attrib.attribute == Attributes.Health)
            {
                return attrib.value;
            }
        }
        return 0;
    }
}
