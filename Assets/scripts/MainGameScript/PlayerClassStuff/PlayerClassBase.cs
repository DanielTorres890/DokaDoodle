using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerClassBase : ScriptableObject
{
    public ItemBuff[] stats;

    public ItemBuff[] levelUpStats;

    
    public abstract void ClassAction();
}
