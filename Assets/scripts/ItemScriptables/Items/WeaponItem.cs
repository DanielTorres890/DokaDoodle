using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Weapon")]
public class WeaponItem : ItemBase
{
    public AttackBase[] attack;
    public List<ItemBuff> skillRequirements;

    public Sprite inHandSprite;

    public BuffBase[] onEquipBuffs;

    [Tooltip("Influences melee attack colors")]
    [ColorUsage(true, true)]
    public Color weaponColor;
    public override void ItemInfoCheck(int player, int itemId)
    {

         if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        var tmp = 3;
       
        if ( this.type == ItemType.Magic)
        {
            tmp = 2;
        }
        if (this.type == ItemType.PhysicalAbility)
        {
            tmp = 1;
        }
        ClientChecks.Instance.ShowConfirmItemButtonsRpc(player, itemId, tmp);
    }
    public override void PerformItemEffect(int player, InventoryObject inventory)
    {
        playerData thisPlayer = NetworkData.Instance.players[player];
        if (thisPlayer.equipItems[this.type] != -1)
        {

            var temp = thisPlayer.equipItems[this.type];
            foreach (var attrib in inventory.database.GetItem[temp].buffs)
            {
                thisPlayer.stats[attrib.attribute] -= attrib.value;

            }
            foreach(var status in (inventory.database.GetItem[temp] as WeaponItem).onEquipBuffs)
            {
                
                thisPlayer.RemoveStatus(NetworkData.Instance.buffDataBase.GetId[status]);
            }
            


        }
        foreach (var attrib in base.buffs)
        {
            thisPlayer.stats[attrib.attribute] += attrib.value;

        }
        foreach (var status in onEquipBuffs)
        {
            thisPlayer.GainStatus(status);
        }

        thisPlayer.equipItems[this.type] = inventory.database.GetId[this];
        inventory.ToFront(this);
        thisPlayer.PostStatusStatCalc();
        var tmp = NetworkData.Instance.playerSticks[player].transform.GetChild(0);
        if (inHandSprite)
        {
            if (this.type == ItemType.Equipment)
            {
                tmp.gameObject.SetActive(true);
                tmp.GetComponent<SpriteRenderer>().sprite = inHandSprite;
            }
            
        }
        else
        {
            tmp.gameObject.SetActive(false);
        }

            NetworkData.Instance.players[player].setCombatActions();
    }
}
