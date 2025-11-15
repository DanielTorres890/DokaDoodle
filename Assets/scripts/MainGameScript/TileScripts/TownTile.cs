using UnityEngine;

public class TownTile : EventTIle
{
    public TownInfo Info;

    public override void TileEvent()
    {
        if (MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner == -1 && MapTileSpecialEvents.Instance.GetCurrentTile().tileEnemy.Count <= 0)
        {
            NetworkData.Instance.GetCurrentPlayer().GainTown(MapTileSpecialEvents.Instance.GetCurrentTile());
        }
        base.TileEvent();
    }
}
