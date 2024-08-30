using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MagicItems : ItemBase
{
    public override void ItemInfoCheck(int player, int itemId)
    {

        if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        ClientChecks.Instance.ConfirmBuffRpc(player, itemId, 1);
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

    }
}
