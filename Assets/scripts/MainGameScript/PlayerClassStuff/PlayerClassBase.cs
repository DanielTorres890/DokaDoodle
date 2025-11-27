using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerClassBase : ScriptableObject
{
    public string className;

    [TextArea(1, 5)] public string classDescription;

    public ItemBuff[] stats;

    public ItemBuff[] levelUpStats;

    public int[] inventorySizes;
    public int ClassActionCD;

    [TextArea(15, 5)]
    public string actionUseText;

    public AttackBase combatAbility;
    public AttackBase basicAttackAbility;

    public int[] classXpRequirements;
    public abstract void ClassAction(playerData player);
    public virtual bool UnlockCondition() { return true; }
}
