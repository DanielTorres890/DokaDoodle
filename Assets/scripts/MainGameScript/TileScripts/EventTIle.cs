using Unity.Netcode;
using UnityEngine;

public class EventTIle : DefaultTile
{
    
    public override void TileEvent()
    {

        if (!NetworkManager.Singleton.IsServer) { return; }

        if(MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Count > 0)
        {
            ClientChecks.Instance.SyncEnemyRpc(0);
            return;
        }
        ClientChecks.Instance.SyncEventRpc(0);
    }
}
