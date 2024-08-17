using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Object", menuName = "Inventory System/Items/Food")]
public class FoodItem : ItemBase
{
    public int restoreHealthValue;
    public void Awake()
    {
        type = ItemType.Food;
    }
}
