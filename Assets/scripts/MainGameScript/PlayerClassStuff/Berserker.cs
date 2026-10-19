using UnityEngine;
[CreateAssetMenu(fileName = "New Default Object", menuName = "PlayerClass/Berserker")]
public class Berserker : PlayerClassBase
{
    public PlayerClassBase prerequisiteClass;


    public BuffBase classBuff;
    public BuffBase resistDeath;
    public override void ClassAction(playerData player, int randomNum)
    {
        player.GainStatus(classBuff, player);
        player.playerInfo[PlayerInfo.classCd] = ClassActionCD;
        base.ClassAction(player, randomNum);
    }
    public override bool UnlockCondition(playerData player)
    {
        int apprenticeId = NetworkData.Instance.classDataBase.GetId[prerequisiteClass];

        if (!player.playerClassProgress.ContainsKey(apprenticeId)) { return false; }
        if (player.playerClassProgress[apprenticeId].level > prerequisiteClass.classXpRequirements.Length - 1) { return true; }
        return false;
    }

    public override void OnCombatStart(AbilityManager myManager)
    {
        myManager.stats.GainStatus(resistDeath, myManager.stats);
        base.OnCombatStart(myManager);
    }
}
