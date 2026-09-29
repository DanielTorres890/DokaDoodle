using UnityEngine;

public class DisplayPassabletile : MonoBehaviour
{
    public TileScript tile;
    public GameObject flag;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        flag.SetActive(!tile.initiallyPassable);

        if (MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber] != null)
        {
 
            flag.SetActive(!MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][tile.tileId].passable);
        }

    }

    
}
