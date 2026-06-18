using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShopUISync : NetworkBehaviour
{
    [SerializeField] private GameObject buyShop;
    [SerializeField] private GameObject sellShop;
    [SerializeField] private List<GameObject> mainMenuButtons;
    [SerializeField] private List<GameObject> buyDontButtons;
    [SerializeField] private List<GameObject> sellDontButtons;
    [SerializeField] private DisplayInventory sellUIManager;
    [SerializeField] private UIStatUpdate moneyDisplay;
    private ShopEvent curEvent;
    public static ShopUISync instance;
    [SerializeField] private ShopUICreator buyShopStuff;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI shopText;
    private void Awake()
    {
        instance = this;
        curEvent = (NetworkData.Instance.currentEvent as ShopEvent);
        if (curEvent.background)
        {
            background.sprite = curEvent.background;
        }
        if (curEvent.backgroundMusic)
        {
            BGMManager.instance.PlaySound(curEvent.backgroundMusic);
        }
    }
    public void BuyButton()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ShowShopRpc();

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowShopRpc()
    {
        buyShopStuff.UpdateDisplay();
        buyShop.SetActive(true);
        hideMenuButtons(mainMenuButtons);
    }
    public void SellButton()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ShowSellRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowSellRpc()
    {
        sellUIManager.CreateDisplay(NetworkData.Instance.currentPlayer);
        sellShop.SetActive(true);
        hideMenuButtons(mainMenuButtons);
    }

    public void EndShop()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EndShopRpc();

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void EndShopRpc()
    {
        hideMenuButtons(mainMenuButtons);
        TileEventManager.Instance.dialogueScript.lines = new List<string>(curEvent.endShopDialogue);
        
        TileEventManager.Instance.EndEvent();
    }
    private void hideMenuButtons(List<GameObject> buttons, bool hide = false)
    {
        foreach (GameObject go in buttons)
        {
            go.SetActive(hide);
        }
    }

    public void setUpBuy(int itemNum)
    {
        if ((!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) || NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo[PlayerInfo.money] < curEvent.itemsSold[itemNum].itemValue) { return; }
        setUpBuyRpc(itemNum);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void setUpBuyRpc(int itemNum)
    {
        var playerInv = NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][curEvent.itemsSold[itemNum].determineType()];
        if (playerInv.container.Count >= playerInv.MAXSIZE)
        {
            shopText.text = "That inventory is full go sell something (remember theres 3 different inventory types :)";
            return;
        }

        buyDontButtons[0].SetActive(true);
        buyDontButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
        var button = buyDontButtons[0].GetComponent<Button>();
        button.Select();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { purchaseItem(itemNum); });

        buyDontButtons[1].SetActive(true);
        var button2 = buyDontButtons[1].GetComponent<Button>();
        buyDontButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Dont Buy";
        button2.onClick.RemoveAllListeners();
        button2.onClick.AddListener(delegate { dontPurchase(); });
        buyShop.SetActive(false);
    }

    private void purchaseItem(int itemNum)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        purchaseItemRpc(itemNum);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void purchaseItemRpc(int itemNum)
    {
        
        NetworkData.Instance.AddItemToInventory(NetworkData.Instance.currentPlayer, curEvent.itemsSold[itemNum]);
        NetworkData.Instance.GetCurrentPlayer().GainMoney(-Mathf.RoundToInt(curEvent.itemsSold[itemNum].itemValue * NetworkData.Instance.globalShopMultiplier));
        moneyDisplay.StatUpdate();
        buyShopStuff.UpdateDisplay();


        if(curEvent.itemsSold[itemNum].determineType() != 3)
        {
            hideMenuButtons(buyDontButtons);
            buyShop.SetActive(true);
        }
        else
        {
            buyDontButtons[0].SetActive(true);
            buyDontButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Equip Bought Equipment";
            var button = buyDontButtons[0].GetComponent<Button>();
            button.Select();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { EquipItem(itemNum); });

            buyDontButtons[1].SetActive(true);
            var button2 = buyDontButtons[1].GetComponent<Button>();
            buyDontButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Dont Equip";
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(delegate { dontPurchase(); });
            buyShop.SetActive(false);
        }
           

        
    }



    private void dontPurchase()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        dontPurchaseRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void dontPurchaseRpc()
    {
        buyShop.SetActive(true);
        hideMenuButtons(buyDontButtons);
    }



    public void setUpSell(int itemNum, int inventoryNum)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        setUpSellRpc(itemNum, inventoryNum);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void setUpSellRpc(int itemNum, int inventoryNum)
    {

        Debug.Log("SELLING BUTTONS SHOULD BE ACTIVE");
        sellShop.SetActive(false);
        sellDontButtons[0].SetActive(true);
        sellDontButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Sell";
        var button = sellDontButtons[0].GetComponent<Button>();
        button.Select();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { sellItem(itemNum, inventoryNum); });

        sellDontButtons[1].SetActive(true);
        var button2 = sellDontButtons[1].GetComponent<Button>();
        sellDontButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Dont Sell";
        button2.onClick.RemoveAllListeners();
        button2.onClick.AddListener(delegate { dontSell(); });
        
    }


    private void sellItem(int itemNum, int inventoryNum)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        sellItemRpc(itemNum, inventoryNum);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void sellItemRpc(int itemNum, int inventoryNum)
    {
        hideMenuButtons(sellDontButtons);
        sellShop.SetActive(true);
        ItemBase whoToSell = NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][inventoryNum].getItem(itemNum);
        if (inventoryNum == 3)
        {
            if (NetworkData.Instance.playerInventories[0][inventoryNum].database.GetId[whoToSell] == NetworkData.Instance.players[NetworkData.Instance.currentPlayer].equipItems[whoToSell.type] ) 
            {

                NetworkData.Instance.players[NetworkData.Instance.currentPlayer].UnequipItem(whoToSell.type);
                var tmp = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform.GetChild(0);
                tmp.gameObject.SetActive(false);
                
            }
        }
        NetworkData.Instance.GetCurrentPlayer().GainMoney(NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][inventoryNum].getItem(itemNum).itemValue / 2);
        NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][inventoryNum].RemoveItem(itemNum);
        moneyDisplay.StatUpdate();
        sellUIManager.CreateDisplay(NetworkData.Instance.currentPlayer, inventoryNum);

    }
    private void dontSell()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        dontSellRpc();
    }
    private void dontSellRpc()
    {
        sellShop.SetActive(true);
        hideMenuButtons(sellDontButtons);
    }
    public void returnToMain()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        returnToMainRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void returnToMainRpc()
    {
        hideMenuButtons(mainMenuButtons, true);
        hideMenuButtons(buyDontButtons);
        buyShop.SetActive(false);
        sellShop.SetActive(false);
    }

    public void EquipItem(int itemIndex)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EquipItemRpc(itemIndex);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void EquipItemRpc(int itemIndex)
    {
        WeaponItem toEquip = curEvent.itemsSold[itemIndex] as WeaponItem;
        toEquip.PerformItemEffect(NetworkData.Instance.currentPlayer, NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][3]);
        hideMenuButtons(buyDontButtons);
        buyShop.SetActive(true);
    }
}
