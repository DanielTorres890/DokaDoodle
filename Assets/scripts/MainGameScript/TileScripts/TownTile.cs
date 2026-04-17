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
        var tileInfo = MapTileSpecialEvents.Instance.GetCurrentTile();
        if (tileInfo.tileEnemy.Count > 0)
        {
            bool noRealEnemies = true;
            foreach (var enemy in tileInfo.tileEnemy)
            {

                if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                {
                    Debug.Log("There was a not matching tag...");
                    noRealEnemies = false;
                }
            }

            if (noRealEnemies)
            {
                ClientChecks.Instance.NoEnemiesToFightRpc(tileId);
                return;
            }

        }

        if (tileInfo.tileOwner == -1 && tileInfo.tileEnemy.Count <= 0)
        {
            NetworkData.Instance.GetCurrentPlayer().GainTown(tileInfo);
        }
        base.TileEvent();
    }
}
