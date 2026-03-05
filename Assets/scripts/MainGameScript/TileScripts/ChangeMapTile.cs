using Unity.Netcode;
using UnityEngine;

public class ChangeMapTile : TileScript
{
    public int mapGoTo;
    public int tileGoTo;
    public override void TileEvent()
    {
        NetworkData.Instance.GetCurrentPlayer().curMap = mapGoTo;
        NetworkData.Instance.GetCurrentPlayer().curTileId = tileGoTo;
        foreach(var ally in NetworkData.Instance.GetCurrentPlayer().partyMembers)
        {
            if(ally.boardMovementState == PlayerFollowingStates.WithOwner)
            {
                ally.curMap = mapGoTo;
                ally.curTileId = tileGoTo;
            }
        }
        if (NetworkManager.Singleton.IsHost) { PlayerMoveManager.Instance.NextTurnRpc(); }
    }
}
