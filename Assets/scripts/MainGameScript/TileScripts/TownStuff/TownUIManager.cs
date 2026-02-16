using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TownUIManager : NetworkBehaviour
{
    public GameObject MainMenu;
    public GameObject LevelMenu;
    public GameObject RestMenu; //okay not really a menu but frick u
    public GameObject AttackMenu;
    public GameObject[] YesNoButtons;
    private TownEvent curTown;
    private SpecialTileEventHold curTownTile;
    public TownLevelManager levelManager; //i gave this a dumb name bc it specifically managgers the level UI
    public UIStatUpdate goldTextManager; //a really generous name to give to something that manages a single text
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI mainMenuText;

    public string attackedSelfMsg, DontOwnMessage, BrokeMsg;
    private void Start()
    {
        curTown = NetworkData.Instance.currentEvent as TownEvent;
        curTownTile = MapTileSpecialEvents.Instance.GetCurrentTile();
        Debug.Log("Current Tile owner " + curTownTile.tileOwner);
    }
    //i could make one script and reuse it for each like i did in the main game menu but that gives me itchiness so im not going to
    //also side note some of these have conditional times to open so i cant just put them on buttons bc there's more layers than that
    //ex: if you own a town you can't attack yourself bc thats dumb
    public void SetMainMenuVisible(bool visibility)
    {
        if(NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetMainMenuVisibleRpc(visibility);
        }

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetMainMenuVisibleRpc(bool visibility)
    {
            MainMenu.SetActive(visibility);

    }

    public void SetInfoTextVisible(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetInfoTextVisibleRpc(visibility);
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetInfoTextVisibleRpc(bool visibility)
    {
            mainMenuText.transform.parent.gameObject.SetActive(visibility);
    }

    public void SetLevelMenuVisible(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            
            SetLevelMenuVisibleRpc(visibility);
            SetMainMenuVisible(!visibility);
            SetInfoTextVisible(!visibility);
        }

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetLevelMenuVisibleRpc(bool visibility)
    {
            LevelMenu.SetActive(visibility);
            
    }

    public void SetRestMenuVisible(bool visibility)
    {
       
        if (MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner != NetworkData.Instance.currentPlayer && NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.money] < curTown.townInfo.restCost)
        {
            //Can't rest do something 
            mainMenuText.text = "You CANNOT afford this";
            return;
        }

        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetRestMenuVisibleRpc(visibility);
            SetMainMenuVisible(!visibility);
        }

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetRestMenuVisibleRpc(bool visibility)
    {
        
            RestMenu.SetActive(visibility);
  

    }


    public void SetAttackMenuVisible(bool visibility)
    {
        if (MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner == NetworkData.Instance.currentPlayer)
        {
            mainMenuText.text = attackedSelfMsg;
            return;

        }
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetAttackMenuVisibleRpc(visibility);
            SetMainMenuVisible(!visibility);
        }

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetAttackMenuVisibleRpc(bool visibility)
    {
        
            AttackMenu.SetActive(visibility);
   

    }

    public void MouseOverMoney()
    {
        infoText.text = "This upgrade increases the weekly earning of this town \nCost: " + Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.townMoneyLevel) + 1) * curTown.townInfo.moneyUpgradeCost).ToString();
    }
    public void MouseOverDefense()
    {
        infoText.text = "This upgrade increases how strong of a troop will protect your town \nCost: " + Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.defenseLevel) + 1) * curTown.townInfo.defenseUpgradeCost).ToString();
    }
    public void MouseOverUnit()
    {
        infoText.text = "This upgrade increases the xp an ally will gain for staying here (allies dont take damage while training on a town) \nCost: " + Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.unitLevel) + 1) * curTown.townInfo.unitUpgradeCost).ToString();
    }
    public void Rest()
    {

        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            RestRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void RestRpc()
    {
        if(MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner != NetworkData.Instance.currentPlayer)
        {
            TownEvent curEvent = NetworkData.Instance.currentEvent as TownEvent;
            if (NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.money] < curEvent.townInfo.restCost)
            {
                //Can't rest do something 
               
                return;
            }
        }

        NetworkData.Instance.GetCurrentPlayer().healHp(99999);
        

        MainMenu.SetActive(false);
        RestMenu.SetActive(false);
        AttackMenu.SetActive(false);
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines = new List<string>() {"Mimimiimimimi" };

        TileEventManager.Instance.EndEvent();

    }


    public void Attack()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            AttackRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void AttackRpc()
    {
        if (MapTileSpecialEvents.Instance.GetCurrentTile().tileOwner == NetworkData.Instance.currentPlayer)
        {
           
            mainMenuText.text = attackedSelfMsg;
            return;
            
        }
        MainMenu.SetActive(false);
        RestMenu.SetActive(false);
        AttackMenu.SetActive(false);
        mainMenuText.text = "WELL TIME TO SQUABBLE";
        PlayerCombatManager.Instance.BattleSetUp(PlayerCombatManager.Instance.EnemyEncounterDataBase.GetId[curTown.townInfo.defenseEncounters[curTownTile.defenseLevel]]);
        StartCoroutine(WaitBeforeMove());
    }
    public void Leave()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            LeaveRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LeaveRpc()
    {
        mainMenuText.transform.parent.gameObject.SetActive(false);
        MainMenu.SetActive(false);
        RestMenu.SetActive(false);
        AttackMenu.SetActive(false);
        
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines = new List<string>(NetworkData.Instance.currentEvent.endDialouge);

        TileEventManager.Instance.EndEvent();
    }
    public void MoneyLevelUpDisplay()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) {  return; }
        if (!NetworkData.Instance.GetCurrentPlayer().CanAfford(Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.townMoneyLevel) + 1) * curTown.townInfo.moneyUpgradeCost))) 
        {
            infoText.text = BrokeMsg;
            return; 
        }


        if (curTownTile.tileOwner == NetworkData.Instance.currentPlayer)
        {
            MoneyLevelUpDisplayRpc();  
        }
        else
        {
            infoText.text = DontOwnMessage;
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void MoneyLevelUpDisplayRpc()
    {
        YesNoButtons[0].transform.parent.gameObject.SetActive(true);
        var buttonComponent = YesNoButtons[0].GetComponent<Button>(); //instead of using a new set of buttons for each on this is probably easier
        
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(LevelUpMoney);

        LevelMenu.SetActive(false);

    }
    public void LevelUpMoney()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.GetCurrentPlayer().CanAfford(Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.townMoneyLevel) + 1) * curTown.townInfo.moneyUpgradeCost))) { return;  } //technically redundant but idgaf
        
        LevelUpMoneyRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LevelUpMoneyRpc()
    {
        NetworkData.Instance.GetCurrentPlayer().GainMoney (-Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.townMoneyLevel) + 1) * curTown.townInfo.moneyUpgradeCost));
        MapTileSpecialEvents.Instance.GetCurrentTile().townMoneyLevel += 1;
        goldTextManager.StatUpdate();
        levelManager.UpdateMoneySlider();
        LevelMenu.SetActive(true);
        YesNoButtons[0].transform.parent.gameObject.SetActive(false);
    }
    public void UnitUpDisplay()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.GetCurrentPlayer().CanAfford(Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.unitLevel) + 1) * curTown.townInfo.unitUpgradeCost)))
        {
            infoText.text = BrokeMsg;
            return;
        }


        if (curTownTile.tileOwner == NetworkData.Instance.currentPlayer)
        {
            UnitUpDisplayRpc();
        }
        else
        {
            infoText.text = DontOwnMessage;
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void UnitUpDisplayRpc()
    {
        YesNoButtons[0].transform.parent.gameObject.SetActive(true);
        var buttonComponent = YesNoButtons[0].GetComponent<Button>(); //instead of using a new set of buttons for each on this is probably easier
        
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(LevelUpUnit);

        LevelMenu.SetActive(false);
    }
    public void LevelUpUnit()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.GetCurrentPlayer().CanAfford(Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.unitLevel) + 1) * curTown.townInfo.unitUpgradeCost))) { return; }

        LevelUpUnitRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LevelUpUnitRpc()
    {
        NetworkData.Instance.GetCurrentPlayer().GainMoney(-Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.unitLevel) + 1) * curTown.townInfo.unitUpgradeCost));
        MapTileSpecialEvents.Instance.GetCurrentTile().unitLevel += 1;
        goldTextManager.StatUpdate();
        levelManager.UpdateUnitSlider();
        LevelMenu.SetActive(true);
        YesNoButtons[0].transform.parent.gameObject.SetActive(false);
    }
    public void DefenseUpDisplay()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.GetCurrentPlayer().CanAfford(Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.defenseLevel) + 1) * curTown.townInfo.defenseUpgradeCost)))
        {
            infoText.text = "You don't gotta enough moneys for this";
            return;
        }


        if (curTownTile.tileOwner == NetworkData.Instance.currentPlayer)
        {
            DefenseUpDisplayRpc();
        }
        else
        {
            Debug.Log("this ish AINT yours");
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DefenseUpDisplayRpc()
    {
        YesNoButtons[0].transform.parent.gameObject.SetActive(true);
        var buttonComponent = YesNoButtons[0].GetComponent<Button>(); //instead of using a new set of buttons for each on this is probably easier

        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(LevelUpDefense);

        LevelMenu.SetActive(false);
    }
    public void LevelUpDefense()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.GetCurrentPlayer().CanAfford(Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.defenseLevel) + 1) * curTown.townInfo.defenseUpgradeCost))) { return; }

        LevelUpDefenseRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LevelUpDefenseRpc()
    {
        NetworkData.Instance.GetCurrentPlayer().GainMoney(-Mathf.RoundToInt(((curTown.townInfo.upgradeCostMultiplier * curTownTile.defenseLevel) + 1) * curTown.townInfo.defenseUpgradeCost));
        MapTileSpecialEvents.Instance.GetCurrentTile().defenseLevel += 1;
        goldTextManager.StatUpdate();
        levelManager.UpdateDefenseSlider();
        LevelMenu.SetActive(true);
        YesNoButtons[0].transform.parent.gameObject.SetActive(false);
    }
    public void ReturnToLevelUp()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ReturnToLevelUpRpc();

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ReturnToLevelUpRpc()
    {
        MainMenu.SetActive(false);
        AttackMenu.SetActive(false);
        LevelMenu.SetActive(true);
        YesNoButtons[0].transform.parent.gameObject.SetActive(false);
    }


    private IEnumerator WaitBeforeMove()
    {
        yield return new WaitForSeconds(3);

        SceneChanger.Instance.loadClientScenesServerRpc(curTownTile.battleArea);
    }
}
