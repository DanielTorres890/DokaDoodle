using System.Linq;
using UnityEngine;

public class TownTile : EventTIle
{
    public TownInfo Info;

    public GameObject flag;


    public override void Start()
    {
        var tileInfo = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][tileId];
        if (tileInfo.tileOwner > -1)
        {
            flag.SetActive(true);
            //long ahh lines
            flag.transform.GetChild(0).GetChild(1).transform.GetComponent<SpriteRenderer>().sprite = NetworkData.Instance.playerSpriteLibrary.GetSprite("hair", NetworkData.Instance.playerSpriteLibrary.GetCategoryLabelNames("hair").ToList()[NetworkData.Instance.players[tileInfo.tileOwner].playerHair]);
            flag.transform.GetChild(0).GetChild(2).transform.GetComponent<SpriteRenderer>().sprite = NetworkData.Instance.playerSpriteLibrary.GetSprite("face", NetworkData.Instance.playerSpriteLibrary.GetCategoryLabelNames("face").ToList()[NetworkData.Instance.players[tileInfo.tileOwner].playerFace]);

        }

        base.Start();
    }
    public override void TileEvent()
    {
        
        if (MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner == -1 && MapTileSpecialEvents.Instance.GetCurrentTile().tileEnemy.Count <= 0)
        {

            NetworkData.Instance.GetCurrentPlayer().GainTown(MapTileSpecialEvents.Instance.GetCurrentTile());
        }
        base.TileEvent();
    }
}
