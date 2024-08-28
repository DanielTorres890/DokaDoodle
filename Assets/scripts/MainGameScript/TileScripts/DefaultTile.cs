using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTile : TileScript
{


    public override void TileEvent()
    {
        PlayerMoveManager.Instance.NextTurnRpc();
    }
}
