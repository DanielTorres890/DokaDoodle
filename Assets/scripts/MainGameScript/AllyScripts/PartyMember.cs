using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class PartyMember : EntityStats
{
    public int allyClass;

    public int allyFace;
    public int allyHair;
    public int allyNumber;
    public int curTileId;
    public int curMap;

    public int allyOwner;

    public int targetTile;

    private int GainMoveLevel = 3;

    public PlayerFollowingStates boardMovementState; 

    public Dictionary<PlayerInfo, int> allyInfo = new Dictionary<PlayerInfo, int>
    {
        {PlayerInfo.xp, 0 },
        {PlayerInfo.level, 1 },
        
    };

    //so to be clear technically i could make moves into a database for them to directly access
    //but that seems like pretty much the same thing as this
    public List<int> weaponsInventory = new List<int>();
    public List<int> magicInventory = new List<int>();




    public PartyMember() 
    {

    
    }

    public PartyMember(int PlayerClass, FixedString32Bytes PlayerName, int PlayerFace, int PlayerHair)
    {
        allyClass = PlayerClass;
        name = PlayerName.ToString();
        allyFace = PlayerFace;
        allyHair = PlayerHair;
        curTileId = 0;
        curMap = 0;
        boardMovementState = PlayerFollowingStates.WithOwner;
        var currentClass = NetworkData.Instance.classDataBase.GetItem[allyClass];
        foreach (var attribute in currentClass.stats)
        {
            stats[attribute.attribute] += attribute.value;
        }

        setCombatActions();
    }

    public void SetPrefab(GameObject prefab)
    {
        if(prefab.TryGetComponent<characterEditor>(out characterEditor editor))
        {
            editor.setClass(allyClass);
            editor.setFace(allyFace);
            editor.setHair(allyHair);
        }
    }
    public void AddAbility(int selected)
    {
        Debug.Log("Step 4");
        var randomItem = NetworkData.Instance.classDataBase.GetItem[allyClass].recommendedItems[selected];
        if(randomItem.determineType() == 1) { weaponsInventory.Add(NetworkData.Instance.playerInventories[0][1].database.GetId[randomItem]); }
        else { magicInventory.Add(NetworkData.Instance.playerInventories[0][2].database.GetId[randomItem]); }

        setCombatActions();
    }


    public void setCombatActions()
    {
        this.attacks.Clear();
        //bool hasOffense = false;

        this.attacks.Add(NetworkData.Instance.classDataBase.GetItem[allyClass].basicAttackAbility);

        for (int i = 0; i < weaponsInventory.Count; i++)
        {
            var thisWeapon = NetworkData.Instance.playerInventories[0][1].database.GetItem[weaponsInventory[i]] as WeaponItem;

            foreach(var attack in thisWeapon.attack)
            {
                if (this.attacks.Contains(attack)) { continue; }
                this.attacks.Add(attack);
            }
            
            


            
        }
        for (int i = 0; i < magicInventory.Count; i++)
        {
            var thisWeapon = NetworkData.Instance.playerInventories[0][2].database.GetItem[magicInventory[i]] as WeaponItem;

            foreach (var attack in thisWeapon.attack)
            {
                if (this.attacks.Contains(attack)) { continue; }
                this.attacks.Add(attack);
            }
        }

        this.attacks.Add(NetworkData.Instance.classDataBase.GetItem[allyClass].combatAbility);

    }
    public void Die()
    {
        NetworkData.Instance.players[allyOwner].partyMembers.Remove(this);
        MapTileSpecialEvents.Instance.mapTiles[curMap][curTileId].partyMembers.Remove(this);
        
    }
    public int gainXp(int xp)
    {
        
        
        int levelsGained = 0;
        this.allyInfo[PlayerInfo.xp] += xp;
        while (this.allyInfo[PlayerInfo.xp] > 24 * Mathf.Pow((float)this.allyInfo[PlayerInfo.level], 2f))
        {
            this.allyInfo[PlayerInfo.level] += 1;
           
            if (allyInfo[PlayerInfo.level] % GainMoveLevel == 0)
            {
                
                int counter = 0;
                foreach (var member in NetworkData.Instance.GetCurrentPlayer().partyMembers)
                {
                    if (member == this) { break; }
                    counter++;
                }

                if (NetworkData.Instance.IsHost)
                    NetworkData.Instance.AddItemToAllyRpc(allyOwner, counter, Random.Range(0, NetworkData.Instance.classDataBase.GetItem[allyClass].recommendedItems.Length));
            }
            levelsGained++;
            foreach (var stat in NetworkData.Instance.classDataBase.GetItem[this.allyClass].levelUpStats)
            {
                this.ChangeBaseStat(stat.attribute, stat.value);

            }
            PostStatusStatCalc();
            
        }
        
        
        return levelsGained;

    }
    

    public bool OffScreenCombat(List<EntityStats> opponents)
    {

        int enemyDamageValue = 0;
        int yourDamageValue = 0;


        bool allyDead = false;
        foreach (var opponent in opponents)
        {

            enemyDamageValue += Mathf.RoundToInt(Mathf.Clamp(
                opponent.stats[Attributes.Attack] * 2 - stats[Attributes.Defense], 0, 1000000) *
                Mathf.Sqrt(Mathf.Log((float)opponent.stats[Attributes.Dexterity] / stats[Attributes.Dexterity] + 1, 2) + 1));




            enemyDamageValue += Mathf.RoundToInt(Mathf.Clamp(
                opponent.stats[Attributes.Magic] * 2 - stats[Attributes.MDefense], 0, 1000000) *
                Mathf.Sqrt(Mathf.Log((float)opponent.stats[Attributes.Dexterity] / stats[Attributes.Dexterity] + 1, 2) + 1));





            yourDamageValue += Mathf.RoundToInt(Mathf.Clamp(
                stats[Attributes.Attack] * 2.5f - opponent.stats[Attributes.Defense], 0, 1000000)  *
                Mathf.Sqrt(Mathf.Log((float)stats[Attributes.Dexterity] / opponent.stats[Attributes.Dexterity] + 1, 2) + 1));




            yourDamageValue += Mathf.RoundToInt(Mathf.Clamp(
                stats[Attributes.Magic] * 2.5f - opponent.stats[Attributes.MDefense], 0, 1000000)  *
                Mathf.Sqrt(Mathf.Log((float)stats[Attributes.Dexterity] / opponent.stats[Attributes.Dexterity] + 1, 2) + 1));


            bool alternate = true;

            
            while (opponent.stats[Attributes.Health] > 0 && stats[Attributes.Health] > 0)
            {
                if(alternate)
                {

                    opponent.isDead = opponent.healHp(-yourDamageValue);
                    
                }
                else
                {
                    
                    allyDead = healHp(-enemyDamageValue);
                }
                alternate = !alternate;

            }
            if(allyDead) 
            {
                Die();
                break; 
            }

            if (opponent is EnemyCombat)
            {
                
                gainXp(PlayerCombatManager.Instance.EnemyDataBase.GetItem[(opponent as EnemyCombat).enemyId].droppedXp);
            }
            else
            {
             
                gainXp((opponent as PartyMember).allyInfo[PlayerInfo.xp] - allyInfo[PlayerInfo.xp]);
            }
        }


        
        return allyDead;
    }
}
public enum PlayerFollowingStates
{
    WithOwner,
    FollowingOwner,
    HoldTile,
   

}

public enum PartyAITypes
{
    basicMelee,
    basicMagic
}