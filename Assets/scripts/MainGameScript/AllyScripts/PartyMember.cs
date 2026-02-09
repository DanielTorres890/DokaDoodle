using System.Collections.Generic;
using Unity.Collections;
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

    public PlayerFollowingStates boardMovementState; 

    public Dictionary<PlayerInfo, int> playerInfo = new Dictionary<PlayerInfo, int>
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

            if (this.attacks.Contains(thisWeapon.attack)) { continue; }


            this.attacks.Add(thisWeapon.attack);
        }
        for (int i = 0; i < magicInventory.Count; i++)
        {
            var thisWeapon = NetworkData.Instance.playerInventories[0][2].database.GetItem[magicInventory[i]] as WeaponItem;
         
            if (this.attacks.Contains(thisWeapon.attack)) { continue; }
            this.attacks.Add(thisWeapon.attack);

        }

        this.attacks.Add(NetworkData.Instance.classDataBase.GetItem[allyClass].combatAbility);

    }
}
public enum PlayerFollowingStates
{
    WithOwner,
    FollowingOwner,

}

public enum PartyAITypes
{
    basicMelee,
    basicMagic
}