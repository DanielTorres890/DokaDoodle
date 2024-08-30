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
    Dexterity
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
    

    public abstract void ItemInfoCheck(int player, int itemId);
    public abstract void PerformItemEffect(int player, InventoryObject inventory);

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
