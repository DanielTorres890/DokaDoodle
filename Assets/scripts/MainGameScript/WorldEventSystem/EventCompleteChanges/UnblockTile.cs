using UnityEngine;

[CreateAssetMenu(fileName = "New Complete Action", menuName = "WorldEvents/CompleteActions/UnblockTile")]
public class UnblockTile : WorldEventComplete
{
    public int tileId;
    public int mapId;

    public override void CompleteAction()
    {
        base.CompleteAction();
        MapTileSpecialEvents.Instance.mapTiles[mapId][tileId].passable = true;

    }
}
