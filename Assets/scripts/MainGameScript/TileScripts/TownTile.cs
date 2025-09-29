using UnityEngine;

public class TownTile : EventTIle
{
    public TownInfo Info;

    public override void TileEvent()
    {
        if (MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner == -1)
        {
            MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner = NetworkData.Instance.currentPlayer;
        }
        base.TileEvent();
    }
}
