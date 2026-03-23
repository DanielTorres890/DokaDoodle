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

    public GameObject myArrow;

    public LineRenderer upRenderer;
    public LineRenderer rightRenderer;
    public LineRenderer downRenderer;
    public LineRenderer leftRenderer;


    public bool canFight = true;
    [Tooltip("This isn't meant to be set manually as their id is decided \nby whatever their place is in the PlayerMoveManager script (bc i cant be bothered to set them manually")]
    public int tileId;
    public string battleEnvironment = "TestArena";
    public List<bool> playersOnTile = new List<bool> { false,false,false,false};

    [Tooltip("When the game first first begins this is what would be spawned in on certain tiles")]
    public EnemyEncounter defaultTileEnemies;
    public bool initiallyPassable = true;

    [TextArea(2,5)]
    public string tileDescription;

    public static bool DrawTrails = true;
    public static float TrailOffset = 0.5f;

    public void Start()
    {
        
        if(DrawTrails)
        {
            Vector3[] trailPositions = new Vector3[2];
            
            //to up tile
            if(upTile)
            {
                Debug.DrawLine(new Vector3(transform.position.x + TrailOffset, transform.position.y, transform.position.z + TrailOffset), new Vector3(upTile.transform.position.x + TrailOffset, upTile.transform.position.y, upTile.transform.position.z - TrailOffset), Color.blue, 99999);
                trailPositions[0] = new Vector3(transform.position.x + TrailOffset, transform.position.y, transform.position.z + TrailOffset);
                trailPositions[1] = new Vector3(upTile.transform.position.x + TrailOffset, upTile.transform.position.y, upTile.transform.position.z - TrailOffset);
                upRenderer.SetPositions(trailPositions);
            }
            else
            {
                upRenderer.enabled = false;
            }
            //to down tile
            if (downTile)
            {
                Debug.DrawLine(new Vector3(transform.position.x - TrailOffset, transform.position.y, transform.position.z - TrailOffset), new Vector3(downTile.transform.position.x - TrailOffset, downTile.transform.position.y, downTile.transform.position.z + TrailOffset), Color.blue, 99999);
                trailPositions[0] = new Vector3(transform.position.x - TrailOffset, transform.position.y, transform.position.z - TrailOffset);
                trailPositions[1] = new Vector3(downTile.transform.position.x - TrailOffset, downTile.transform.position.y, downTile.transform.position.z + TrailOffset);
                downRenderer.SetPositions(trailPositions);
            }
            else
            {
                downRenderer.enabled = false;
            }
            //to right tile
            if (rightTile)
            {

                Debug.DrawLine(new Vector3(transform.position.x + TrailOffset, transform.position.y, transform.position.z - TrailOffset), new Vector3(rightTile.transform.position.x - TrailOffset, rightTile.transform.position.y, rightTile.transform.position.z - TrailOffset), Color.blue, 99999);

                trailPositions[0] = new Vector3(transform.position.x + TrailOffset, transform.position.y, transform.position.z - TrailOffset);
                trailPositions[1] = new Vector3(rightTile.transform.position.x - TrailOffset, rightTile.transform.position.y, rightTile.transform.position.z - TrailOffset);
                rightRenderer.SetPositions(trailPositions);
            }
            else
            {
                rightRenderer.enabled = false;
            }
            //to left tile
            if (leftTile)
            {
                Debug.DrawLine(new Vector3(transform.position.x - TrailOffset, transform.position.y, transform.position.z + TrailOffset), new Vector3(leftTile.transform.position.x + TrailOffset, leftTile.transform.position.y, leftTile.transform.position.z + TrailOffset), Color.blue, 99999);
                trailPositions[0] = new Vector3(transform.position.x - TrailOffset, transform.position.y, transform.position.z + TrailOffset);
                trailPositions[1] = new Vector3(leftTile.transform.position.x + TrailOffset, leftTile.transform.position.y, leftTile.transform.position.z + TrailOffset);
                leftRenderer.SetPositions(trailPositions);

            }
            else
            {
                leftRenderer.enabled = false;
            }

        }
    }


    public abstract void TileEvent();
    
    public void ArrowChange(bool setTo)
    {
        myArrow.SetActive(setTo);
    }
    /*
    public void Move(GameObject player, FixedString32Bytes direction)
    {
        if (direction == "up") { player.transform.position = Vector3.MoveTowards(player.transform.position, upTile.transform.position, 3 * Time.deltaTime) }
    }
    */
}
