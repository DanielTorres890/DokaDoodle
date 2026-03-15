using System.Collections;
using System.Collections.Generic;
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
    Potency,
    PDmgReduction,
    MDmgReduction

}
public abstract class ItemBase : ScriptableObject
{
    public Sprite itemSprite;
    public ItemType type;
    public int itemValue;

    [TextArea(15,20)]
    public string description;
    [TextArea(15, 10)]
    public string useText;
    public string itemName;
    public ItemBuff[] buffs;

    public bool battleItem = false;
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
        if (this.type == ItemType.Food) {return 0; }

        if (this.type == ItemType.PhysicalAbility || this.type == ItemType.Shield) { return 1; }

        if (this.type == ItemType.Magic ) { return 2; }

        if (this.type == ItemType.Equipment) { return 3; }

        return -1;
    }
    public virtual bool CanUse(int player)
    {
        return true;
    }
    public virtual void InCombatAction(AbilityManager user)
    {

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
