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
    public int curMap;

    public int[] maxInventorySizes = new int[3];

    public Dictionary<string, int> playerInfo = new Dictionary<string, int>
    {
        {"xp", 0 },
        {"level", 1 },
        {"money", 2000 },
        {"fame", 0 }
    };
    
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

        
    } 
    
    public void setCombatActions()
    {
        this.attacks.Clear();
        
        if(NetworkData.Instance.playerInventories[playerNumber][1].container.Count == 0  && NetworkData.Instance.playerInventories[playerNumber][2].container.Count == 0)
        {
            this.attacks.Add((NetworkData.Instance.playerInventories[playerNumber][1].database.GetItem[0] as WeaponItem).attack);
        }
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][1].container.Count; i++)
        {
            if (this.attacks.Contains((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem).attack)) { continue; }
            this.attacks.Add((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem).attack);
        }

        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][2].container.Count; i++)
        {
            if (this.attacks.Contains((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem).attack)) { continue; }
            this.attacks.Add((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem).attack);
        }

        this.attacks.Add(NetworkData.Instance.classDataBase.GetClass[playerClass].combatAbility);
        // this.defenses[0] = (NetworkData.Instance.playerInventories[0][1].database.GetItem[this.equipItems[ItemType.Shield]] as WeaponItem).attack as DefenseBase;
        // this.defenses[1] = (NetworkData.Instance.playerInventories[0][2].database.GetItem[this.equipItems[ItemType.MagicGuard]] as WeaponItem).attack as DefenseBase; 
    }
    public FixedString32Bytes getName()
    {
        return name;
    }
   
    public string LoseSomething()
    {
        string whatwaslost = "nothing was lost u (" + name + ") lucky son of a gun";
        int whattolose = Random.Range(1, 30);
        if (whattolose > 0 && whattolose < 50)
        {

            for (int k = 0; k < NetworkData.Instance.playerInventories[this.playerNumber].Count; k++)
            {

                if (NetworkData.Instance.playerInventories[this.playerNumber][k].container.Count > 0)
                {

                    int itemLost = Random.Range(0, NetworkData.Instance.playerInventories[this.playerNumber][k].container.Count);

                    whatwaslost = "Lost <color=red>" + NetworkData.Instance.playerInventories[playerNumber][k].container[itemLost].item.name + "</color>";
                    NetworkData.Instance.LoseItemRpc(playerNumber,itemLost,k);
                    ;

                }
            }


        }
       
        return whatwaslost;
    }
    public void death(int turnsDead = -1, bool backToBase = true)
    {
        this.isDead = true;
        this.tillRevive = turnsDead;

        Debug.Log(this.name + "man i should reallllyy be dead " + this.isDead); 
        if (backToBase)
        {
            this.curTileId = this.playerSpawnTile;
            MapTileSpecialEvents.Instance.mapTiles[curMap][curTileId].players.Remove(playerNumber);

        }

        if (turnsDead == -1)
        {
            this.tillRevive = Random.Range(2, 2);
        }

    }
    public void progressDeath()
    {
        this.tillRevive--;
        if (tillRevive <= 0)
        {

            isDead = false;
            tillRevive = 0;
            this.stats[Attributes.Health] = this.stats[Attributes.MaxHealth];
        }
    }

    public int gainXp(int xp)
    {
        int levelsGained = 0;
        this.playerInfo["xp"] += xp;
        while (this.playerInfo["xp"] > 24 * Mathf.Pow((float)this.playerInfo["level"], 1.2f) + 20) 
        {
            this.playerInfo["level"] += 1;
            levelsGained++;
            foreach( var stat in NetworkData.Instance.classDataBase.Classes[this.playerClass].levelUpStats)
            {
                this.ChangeBaseStat(stat.attribute, stat.value);

            }
            PostStatusStatCalc();

        }

        return levelsGained;

    }
    public void healHp(int hp)
    {
        if (this.stats[Attributes.Health] +  hp > this.stats[Attributes.MaxHealth]) 
        {
            this.stats[Attributes.Health] = this.stats[Attributes.MaxHealth];
        }
        else
        {
            this.stats[Attributes.Health] += hp;
        }
    }
    public void UnequipItem(ItemType type)
    {
        var temp = NetworkData.Instance.players[playerNumber].equipItems[type];
        NetworkData.Instance.players[playerNumber].equipItems[type] = 0;

        InventoryObject temp2;
        if (type == ItemType.Magic || type == ItemType.MagicGuard)
        {
            temp2 = NetworkData.Instance.playerInventories[playerNumber][2];
        }
        else
        {
            temp2 = NetworkData.Instance.playerInventories[playerNumber][1];
        }

        foreach (var attrib in temp2.database.GetItem[temp].buffs)
        {
            NetworkData.Instance.players[playerNumber].stats[attrib.attribute] -= attrib.value;

        }
    }
}
