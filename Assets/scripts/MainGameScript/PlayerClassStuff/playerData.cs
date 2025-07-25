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
    

    public Dictionary<PlayerInfo, int> playerInfo = new Dictionary<PlayerInfo, int>
    {
        {PlayerInfo.xp, 0 },
        {PlayerInfo.level, 1 },
        {PlayerInfo.money, 0 },
        {PlayerInfo.fame, 0 },
        {PlayerInfo.classCd, 0 }
   
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
        bool hasOffense = false;
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][1].container.Count; i++)
        {
            
            if (!UsableItem((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem))) { continue; }

            if ((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem).attack is not GuardAbility) { hasOffense = true; }
            
        }
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][2].container.Count; i++)
        {
            if (!UsableItem((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem))) { continue; }

            if ((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem).attack is not GuardAbility) { hasOffense = true; }

        }

        if (!hasOffense)
        {
            this.attacks.Add((NetworkData.Instance.playerInventories[playerNumber][1].database.GetItem[0] as WeaponItem).attack);
        }
        //all the stuff above is checking if the player actually has an item that provides offense bc if they don't you're helpless for no reason

        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][1].container.Count; i++)
        {
            
            if (!UsableItem((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem))) { continue; }

            if (this.attacks.Contains((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem).attack)) { continue; }

            this.attacks.Add((NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem).attack);
        }
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][2].container.Count; i++)
        {
            if (!UsableItem((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem))) { continue; }

            if (this.attacks.Contains((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem).attack)) { continue; }
            this.attacks.Add((NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem).attack);
        }

        this.attacks.Add(NetworkData.Instance.classDataBase.GetItem[playerClass].combatAbility);
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

        
        if (backToBase)
        {
            
            
            bool success = MapTileSpecialEvents.Instance.mapTiles[curMap][curTileId].players.Remove(playerNumber);
            
            this.curTileId = this.playerSpawnTile;    

        }

        if (turnsDead == -1)
        {
            this.tillRevive = Random.Range(2, 2); //man random numbers suck buns
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
        this.playerInfo[PlayerInfo.xp] += xp;
        while (this.playerInfo[PlayerInfo.xp] > 24 * Mathf.Pow((float)this.playerInfo[PlayerInfo.level], 1.2f) + 20) 
        {
            this.playerInfo[PlayerInfo.level] += 1;
            levelsGained++;
            foreach( var stat in NetworkData.Instance.classDataBase.GetItem[this.playerClass].levelUpStats)
            {
                this.ChangeBaseStat(stat.attribute, stat.value);

            }
            PostStatusStatCalc();

        }

        return levelsGained;

    }
    public bool healHp(int hp) //note this will work for dmg too ig
    {
        if (this.stats[Attributes.Health] +  hp > this.stats[Attributes.MaxHealth]) 
        {
            this.stats[Attributes.Health] = this.stats[Attributes.MaxHealth];
        }
        else
        {
            this.stats[Attributes.Health] += hp;
        }
        PostStatusStatCalc();
        if(stats[Attributes.Health] <= 0)
        {
            return true;
        }
        return false;
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
    private bool UsableItem(WeaponItem item)
    {
        bool canUse = true;
        foreach (var attribs in item.skillRequirements)
        {
            if (stats[attribs.attribute] < attribs.value) { canUse = false; break; }
        }
        return canUse;
        
    }
}
public enum PlayerInfo
{
    xp,
    level,
    money,
    fame,
    classCd
}

[System.Serializable]
public class PlayerClassProgress
{
    public int xp = 0;
    public int level = 0;

}
