using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public class playerData : EntityStats
{

    public int playerClass;

    public int playerFace;
    public int playerHair;
    public int playerNumber;
    public int curTileId;
    public int totalXp;
    public int curMap;

    public bool isDead;
    public int tillRevive;

    public int playerSpawnTile;

    public Dictionary<ItemType, int> equipItems = new Dictionary<ItemType, int>
    {
        { ItemType.Equipment , 0 },
        { ItemType.Weapon,  0},
        { ItemType.Magic , 0 },
        { ItemType.Shield, 1 },
        { ItemType.MagicGuard, 1 }


    };
    
    public playerData()
    {
        playerClass = 0;
        name = "";
        playerFace = 0;
        playerHair = 0;
        curTileId = 0;
        curMap = 0;
        playerSpawnTile = 0;
        setCombatActions();
    }
    public playerData(int PlayerClass, FixedString32Bytes PlayerName, int PlayerFace, int PlayerHair)
    {
        playerClass = PlayerClass;
        name = PlayerName.ToString();
        playerFace = PlayerFace;
        playerHair = PlayerHair;
        curTileId = 0;
        curMap = 0;
        playerSpawnTile = 0;
        setCombatActions();
    } 
    
    public void setCombatActions()
    {
        this.attacks[0] = (NetworkData.Instance.playerInventories[0][1].database.GetItem[this.equipItems[ItemType.Weapon]] as WeaponItem).attack;
        this.attacks[1] = (NetworkData.Instance.playerInventories[0][2].database.GetItem[this.equipItems[ItemType.Magic]] as WeaponItem).attack;
        this.defenses[0] = (NetworkData.Instance.playerInventories[0][1].database.GetItem[this.equipItems[ItemType.Shield]] as WeaponItem).attack as DefenseBase;
        this.defenses[1] = (NetworkData.Instance.playerInventories[0][2].database.GetItem[this.equipItems[ItemType.MagicGuard]] as WeaponItem).attack as DefenseBase;
    }
    public FixedString32Bytes getName()
    {
        return name;
    }
   
    public string LoseSomething()
    {
        string whatwaslost = "nothing was lost u lucky son of a gun";
        int whattolose = Random.Range(0, 100);
        if (whattolose > 0 && whattolose < 50)
        {

            for (int k = 0; k < NetworkData.Instance.playerInventories[this.playerNumber].Count; k++)
            {

                if (NetworkData.Instance.playerInventories[this.playerNumber][k].container.Count > 0)
                {
                    int itemLost = Random.Range(0, NetworkData.Instance.playerInventories[this.playerNumber][k].container.Count);
                    whatwaslost = "Lost <color=red>" + NetworkData.Instance.playerInventories[this.playerNumber][k].container[whattolose].item.name + "</color>";
                    NetworkData.Instance.playerInventories[this.playerNumber][k].container.RemoveAt(itemLost);

                    break;
                }
            }


        }
       
        return whatwaslost;
    }
    public void death(int turnsDead = -1)
    {
        this.isDead = true;
        this.tillRevive = turnsDead;
        Debug.Log(this.name + "man i should reallllyy be dead " + this.isDead); ;
        this.curTileId = this.playerSpawnTile;
        if (turnsDead == -1)
        {
            this.tillRevive = Random.Range(3, 3);
        }

    }
    public void progressDeath()
    {
        tillRevive--;
        if (tillRevive <= 0)
        {
            Debug.Log("somehow this happened?");
            isDead = false;
            tillRevive = 0;
            this.stats[Attributes.Health] = this.stats[Attributes.MaxHealth];
        }
    }
}
