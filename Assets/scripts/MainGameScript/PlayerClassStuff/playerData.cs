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
        {PlayerInfo.money, 1000 },
        {PlayerInfo.fame, 0 },
        {PlayerInfo.classCd, 0 }
   
    };
    
    

    public int tillRevive;

    public int playerSpawnTile;

    //this may get scrapped at somep point
    public Dictionary<ItemType, int> equipItems = new Dictionary<ItemType, int>
    {
        { ItemType.Equipment , -1 },
        { ItemType.Weapon,  -1},
        { ItemType.Magic , -1 },
        { ItemType.Shield, -1 },
        { ItemType.MagicGuard, -1 }


    };

    public Dictionary<int,int> ownedTowns = new Dictionary<int, int>();

    //should add new ones as they're unlocked instead of them already existing
    //but every player always has these 3
    //and incase we're worried about imaginary numbers the int represents the class id
    public Dictionary<int,PlayerClassProgress> playerClassProgress = new Dictionary<int, PlayerClassProgress>
    {
        { 0, new PlayerClassProgress() },
        { 1, new PlayerClassProgress() },
        { 2, new PlayerClassProgress() },

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
        //bool hasOffense = false;
        this.attacks.Add(NetworkData.Instance.classDataBase.GetItem[playerClass].basicAttackAbility);

        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][1].container.Count; i++)
        {
            var thisWeapon = NetworkData.Instance.playerInventories[playerNumber][1].getItem(i) as WeaponItem;

            if (!UsableItem(thisWeapon)) { continue; }
            if (!thisWeapon.attack.meetsRequirement(this)) { continue; }

            if (this.attacks.Contains(thisWeapon.attack)) { continue; }

            Debug.Log("This attack was added " + thisWeapon.attack.attackName);
            this.attacks.Add(thisWeapon.attack);
        }
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerNumber][2].container.Count; i++)
        {
            var thisWeapon = NetworkData.Instance.playerInventories[playerNumber][2].getItem(i) as WeaponItem;
            if (!UsableItem(thisWeapon)) { continue; }

            if (this.attacks.Contains(thisWeapon.attack)) { continue; }
            this.attacks.Add(thisWeapon.attack);
            Debug.Log("This attack was added " + thisWeapon.attack.attackName);
        }

        this.attacks.Add(NetworkData.Instance.classDataBase.GetItem[playerClass].combatAbility);

    }
    public FixedString32Bytes getName()
    {
        return name;
    }
   
    public ItemBase LoseSomething()
    {
        int whattolose = Random.Range(1, 30);
        if (whattolose > 0 && whattolose < 50)
        {

            for (int k = 0; k < NetworkData.Instance.playerInventories[this.playerNumber].Count; k++)
            {

                if (NetworkData.Instance.playerInventories[this.playerNumber][k].container.Count > 0)
                {

                    int itemLost = Random.Range(0, NetworkData.Instance.playerInventories[this.playerNumber][k].container.Count);
                    ItemBase item = NetworkData.Instance.playerInventories[this.playerNumber][k].container[itemLost].item;
                    NetworkData.Instance.LoseItemRpc(playerNumber,itemLost,k);
                    return item;
                    ;

                }
            }


        }
       
        return null;
    }
    public void death(int turnsDead = -1, bool backToBase = true)
    {
        this.isDead = true;
        this.tillRevive = turnsDead;
        this.stats[Attributes.Health] = 0;
        
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
    public int gainClassXp(int xp)
    {

        if(this.playerClassProgress[playerClass].level >= NetworkData.Instance.classDataBase.GetItem[playerClass].classXpRequirements.Length) { return 0; }


        this.playerClassProgress[playerClass].xp += xp;
        if (this.playerClassProgress[playerClass].xp >= NetworkData.Instance.classDataBase.GetItem[playerClass].classXpRequirements[this.playerClassProgress[playerClass].level])
        {
            this.playerClassProgress[playerClass].level += 1;
            Debug.Log("Class level up :)");
            return 1;
        }
        return 0;
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

    public void GainMoney(int amount)
    {
        playerInfo[PlayerInfo.money] += amount;
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
    public bool CanAfford(int cost)
    {
        return playerInfo[PlayerInfo.money] >= cost;
    }

    public void GainTown(SpecialTileEventHold tileInfo)
    {
        if(tileInfo.tileOwner != -1)
        {
            NetworkData.Instance.players[tileInfo.tileOwner].LoseTown(tileInfo);
        }
        int TileId = -1;
        //id like to say that for the record this depresses me
        for (int i = 0; i < MapTileSpecialEvents.Instance.mapTiles[0].Length; i++)
        {
            if (MapTileSpecialEvents.Instance.mapTiles[0][i] == tileInfo)
            {
                TileId = i;
                break;
            }
        }

        Debug.Log("who tf " + tileInfo.townId);
        Debug.Log("what be this " + tileInfo.townMoneyLevel + tileInfo.unitLevel + tileInfo.defenseLevel);
        ownedTowns.Add(tileInfo.townId, TileId);
        playerInfo[PlayerInfo.fame] += NetworkData.Instance.TownInfoDataBase.GetItem[tileInfo.townId].baseFame;
        playerInfo[PlayerInfo.fame] += tileInfo.townMoneyLevel + tileInfo.unitLevel + tileInfo.defenseLevel;

    }
    public void LoseTown(SpecialTileEventHold tileInfo)
    {
        ownedTowns.Remove(tileInfo.townId);
        playerInfo[PlayerInfo.fame] -= NetworkData.Instance.TownInfoDataBase.GetItem[tileInfo.townId].baseFame;
        playerInfo[PlayerInfo.fame] -= tileInfo.townMoneyLevel + tileInfo.unitLevel + tileInfo.defenseLevel;
    }
    public void ChangeClass(PlayerClassBase classChangeTo)
    {
        var currentClass = NetworkData.Instance.classDataBase.GetItem[playerClass];
        foreach(var attrib in currentClass.stats)
        {
            stats[attrib.attribute] -= attrib.value;
        }
        foreach(var attrib in classChangeTo.stats)
        {
            stats[attrib.attribute] += attrib.value;
        }
        playerClass = NetworkData.Instance.classDataBase.GetId[classChangeTo];
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
