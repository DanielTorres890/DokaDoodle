using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpecialTileEventHold 
{
    public List<EnemyCombat> tileEnemy = new List<EnemyCombat>();

    //so this is seperate bc they function juussstt differently enough to where i think it should be different
    //but i can see an argument where i should merge tileEnemy and party members
    public List<PartyMember> partyMembers = new List<PartyMember>();

    public List<int> players = new List<int>();

    public List<int> trapIds = new List<int>();//traps active on this

    public string battleArea;

    public bool passable = true;
    public int xpOnTile;
    public int moneyOnTile;



    public int tileOwner = -1;
    public int townId;
    public int townMoneyLevel; //im not sure if this is a great spot for it but it'll have to do since i use this class for persistant tile data
    public int defenseLevel;
    public int unitLevel; //basically you'll be able to station a guy at a town then they can train up until whatever the level of the town is
    

}
