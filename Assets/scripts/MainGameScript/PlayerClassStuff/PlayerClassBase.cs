using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class PlayerClassBase : ScriptableObject
{
    public string className;
    public int classTier;
    [TextArea(1, 5)] public string classDescription;
    [TextArea(1, 5)] public string overworldAbilityDescription;

    public ItemBuff[] stats;

    public ItemBuff[] levelUpStats;

    public int[] inventorySizes;
    public int ClassActionCD;


    public string classActionName;
    [TextArea(15, 5)]
    public string actionUseText;
    public ClassAbilityType actionType;

    public AttackBase combatAbility;
    public AttackBase basicAttackAbility;

    
    public int[] classXpRequirements;

    public int baseSalary;

    public WeaponItem[] recommendedItems;
    public PlayerClassBase[] allyClassUpgrades;

    [TextArea(3,6)]
    public string allyUnlockTips;

    public PartyAITypes AIType;
    public virtual void ClassAction(playerData player, int randomNum)
    {
        player.playerInfo[PlayerInfo.classCd] = ClassActionCD;
        if (NetworkData.Instance.IsHost) { ClientChecks.Instance.CompleteClassAbilityRpc(); }
    }
    public virtual bool UnlockCondition(playerData player) { return true; }

    public virtual bool AllyUnlockCondition(PartyMember ally) { return ally.allyInfo[PlayerInfo.level] >= (classTier * 10); }

}

public enum ClassAbilityType
{
    Combat,
    Movement,
    Other
}