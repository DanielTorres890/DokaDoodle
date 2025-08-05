using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerClassBase : ScriptableObject
{
    public ItemBuff[] stats;

    public ItemBuff[] levelUpStats;

    public int[] inventorySizes;
    public int ClassActionCD;

    [TextArea(15, 5)]
    public string actionUseText;

    public AttackBase combatAbility;
    public AttackBase basicAttackAbility;
    public abstract void ClassAction(playerData player);
}
