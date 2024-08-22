using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    Food,
    Equipment,
    Weapon,
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
    public ItemBuff[] buffs;

    public abstract void PerformItemEffect(int player, ref InventoryObject inventory);

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
