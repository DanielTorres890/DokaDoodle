using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

    public int baseSalary;
    public virtual void ClassAction(playerData player, int randomNum)
    {
        player.playerInfo[PlayerInfo.classCd] = ClassActionCD;
        if (NetworkData.Instance.IsHost) { ClientChecks.Instance.CompleteClassAbilityRpc(); }
    }
    public virtual bool UnlockCondition(playerData player) { return true; }
}
