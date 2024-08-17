using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesetScript : MonoBehaviour
{
    public List<TileScript> tiles = new List<TileScript>();

    public TilesetScript(List<TileScript> tiles = null)
    {
        if(tiles != null)
        {
            foreach(TileScript tile in tiles)
            {
                this.tiles.Add(tile);
            }
        }
    }

    public TileScript GetPreviousTile()
    {
        if(tiles.Count > 0) { return tiles[tiles.Count-2]; }
        return null;
    }
}
