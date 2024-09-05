using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class TileScript : MonoBehaviour 
{
    public GameObject upTile;
    public GameObject downTile;
    public GameObject rightTile;
    public GameObject leftTile;

    public EnemyCombat tileEnemy;
    public int tileId;
    public List<bool> playersOnTile = new List<bool> { false,false,false,false};

    public abstract void TileEvent();
    
    /*
    public void Move(GameObject player, FixedString32Bytes direction)
    {
        if (direction == "up") { player.transform.position = Vector3.MoveTowards(player.transform.position, upTile.transform.position, 3 * Time.deltaTime) }
    }
    */
}
