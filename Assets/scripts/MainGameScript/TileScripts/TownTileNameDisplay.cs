using TMPro;
using UnityEngine;

public class TownTileNameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private TownTile TownTile;
    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        
        text.text = TownTile.Info.TownName;
        var tileOwnerId = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][TownTile.tileId].tileOwner;
        if (tileOwnerId > -1)
        {
            text.text = TownTile.Info.TownName + " (" + NetworkData.Instance.players[tileOwnerId].name + ")";
        }
    }
}
