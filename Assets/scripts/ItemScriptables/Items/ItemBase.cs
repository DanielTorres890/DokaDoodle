using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    Food,
    Equipment,
    Weapon,
    Shield,
    Magic,
    MagicGuard,
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
    public abstract void ItemInfoCheck(int player, int itemId);
    public abstract void PerformItemEffect(int player, InventoryObject inventory);
    public int determineType ()
    {
        if (this.type == ItemType.Food) {return 0; }

        if (this.type == ItemType.Weapon || this.type == ItemType.Shield) { return 1; }

        if (this.type == ItemType.Magic || this.type == ItemType.Equipment) { return 2; }
        return -1;
    }
    public virtual bool CanUse(int player)
    {
        return true;
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
