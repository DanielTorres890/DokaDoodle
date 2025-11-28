using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Default Object", menuName = "PlayerClass/Hunter")]
public class Hunter : PlayerClassBase
{

    public BuffBase classBuff;
    

    public override void ClassAction(playerData player, int randomNum)
    {
        player.GainStatus(classBuff);
        player.playerInfo[PlayerInfo.classCd] = ClassActionCD;
        base.ClassAction(player, randomNum);
    }
}
