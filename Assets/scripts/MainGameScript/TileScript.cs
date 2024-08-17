using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public GameObject upTile;
    public GameObject downTile;
    public GameObject rightTile;
    public GameObject leftTile;

    public int tileId;


    
    /*
    public void Move(GameObject player, FixedString32Bytes direction)
    {
        if (direction == "up") { player.transform.position = Vector3.MoveTowards(player.transform.position, upTile.transform.position, 3 * Time.deltaTime) }
    }
    */
}
