using UnityEngine;
[CreateAssetMenu(fileName = "New Default Object", menuName = "PlayerClass/Wizard")]
public class Wizard : PlayerClassBase
{
    public PlayerClassBase prerequisiteClass;


    public BuffBase[] possibleBuffs;
    public override void ClassAction(playerData player, int randomNum)
    {
        player.GainStatus(possibleBuffs[randomNum % possibleBuffs.Length]);
        player.playerInfo[PlayerInfo.classCd] = ClassActionCD;
        base.ClassAction(player, randomNum);
    }
    public override bool UnlockCondition(playerData player)
    {
        int apprenticeId = NetworkData.Instance.classDataBase.GetId[prerequisiteClass];
        if (player.playerClassProgress[apprenticeId].level > prerequisiteClass.classXpRequirements.Length - 1) { return true;}
        return false;
    }
    
}
