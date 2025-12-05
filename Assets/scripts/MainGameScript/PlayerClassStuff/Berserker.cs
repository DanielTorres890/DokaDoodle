using UnityEngine;
[CreateAssetMenu(fileName = "New Default Object", menuName = "PlayerClass/Berserker")]
public class Berserker : PlayerClassBase
{
    public PlayerClassBase prerequisiteClass;


    public BuffBase classBuff;
    public override void ClassAction(playerData player, int randomNum)
    {
        player.GainStatus(classBuff);
        player.playerInfo[PlayerInfo.classCd] = ClassActionCD;
        base.ClassAction(player, randomNum);
    }
    public override bool UnlockCondition(playerData player)
    {
        int apprenticeId = NetworkData.Instance.classDataBase.GetId[prerequisiteClass];
        if (player.playerClassProgress[apprenticeId].level > prerequisiteClass.classXpRequirements.Length - 1) { return true; }
        return false;
    }
}
