using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerClassBase : ScriptableObject
{
    public ItemBuff[] stats;

    public abstract void ClassAction();
}
