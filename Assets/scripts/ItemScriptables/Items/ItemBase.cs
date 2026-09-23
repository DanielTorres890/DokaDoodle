using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public enum ItemType
{
    Food,
    Equipment,
    Weapon,
    PhysicalAbility,
    Shield,
    Magic,
    Default

}

public enum Attributes
{
    
    MaxHealth,
    Health,
    Attack,
    Defense,
    Magic,
    MDefense,
    Dexterity,

    PDmgReduction,
    MDmgReduction

}
public abstract class ItemBase : ScriptableObject
{
    public Sprite itemSprite;
    public ItemType type;
    public int itemValue;

    [TextArea(5,20)]
    public string description;
    [TextArea(5, 10)]
    public string useText;
    public string itemName;
    public ItemBuff[] buffs;

    public bool battleItem = false;
    [Tooltip("only needs to be filled in if its usable in combat")]
    public AttackBase inCombatAttack;


    public bool overworldItem = true;
    public AudioClip useClip;
    
    public bool interrupt = false;
    public abstract void ItemInfoCheck(int player, int itemId);
    public virtual void PerformItemEffect(int player, InventoryObject inventory)
    {
        if(battleItem)
        {
            foreach (var item in NetworkData.Instance.playerInventories[player][0].container)
            {
                if (item.item == this)
                {
                    NetworkData.Instance.players[player].battleSlotItemId = -1;
                }
            }
        }
        
        inventory.RemoveItem(this);
    }
    public int determineType ()
    {
       

        if (this.type == ItemType.PhysicalAbility || this.type == ItemType.Shield) { return 1; }

        else if (this.type == ItemType.Magic ) { return 2; }

        else if (this.type == ItemType.Equipment) { return 3; }

        else { return 0; }
        
    }
    public virtual bool CanUse(int player)
    {
        return true;
    }
    public virtual void InCombatAction(AbilityManager user)
    {
        if (!NetworkManager.Singleton.IsHost) { return; }
        if(!inCombatAttack) { return; }

        Debug.Log("did i attempt an action? ");
        inCombatAttack.WeaponEffect(user.gameObject, Time.time, user.transform.position, user.transform.eulerAngles, 0f, user.transform.position, user.transform.eulerAngles);
    }
}
[System.Serializable]
public class ItemBuff
{
    public Attributes attribute;
    public int value;
    public ItemBuff(int num)
    {
        value = num;
    }
}
