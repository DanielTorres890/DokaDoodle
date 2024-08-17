using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Object", menuName = "Inventory System/Items/Equipment")]
public class EquipmentItem : ItemBase
{
    public float atkBonus;
    public float defenceBonus;
    public float magicBonus;
    public float mDefenseBonus;
    public float dexterityBonus;
    public float healthBonus;
    public void Awake()
    {
        type = ItemType.Equipment;
    }
}
