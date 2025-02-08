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
    private ShopEvent curEvent;
    public static ShopUISync instance;

    private void Awake()
    {
        instance = this;
        curEvent = (NetworkData.Instance.currentEvent as ShopEvent);
    }
    public void BuyButton()
    {
        if(!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkManager.Singleton.LocalClientId)) { return;  }
        ShowShopRpc();

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ShowShopRpc()
    {
        buyShop.SetActive(true);
        hideMenuButtons(mainMenuButtons);
    }
    public void SellButton()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ShowSellRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ShowSellRpc()
    {
        sellShop.SetActive(true);
        hideMenuButtons(mainMenuButtons);
    }

    public void EndShop()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EndShopRpc();

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
        if ((!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) || NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo["money"] < curEvent.itemsSold[itemNum].itemValue) { return; }
        setUpBuyRpc(itemNum);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void setUpBuyRpc(int itemNum)
    {
       
        buyDontButtons[0].SetActive(true);
        buyDontButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Sell";
        var button = buyDontButtons[0].GetComponent<Button>();
        button.Select();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { purchaseItem(itemNum); });

        buyDontButtons[1].SetActive(true);
        var button2 = buyDontButtons[1].GetComponent<Button>();
        button2.onClick.RemoveAllListeners();
        button2.onClick.AddListener(delegate { dontPurchase(); });
        buyShop.SetActive(false);
    }

    private void purchaseItem(int itemNum)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        purchaseItemRpc(itemNum);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void purchaseItemRpc(int itemNum)
    {
        NetworkData.Instance.AddItemToInventory(NetworkData.Instance.currentPlayer, curEvent.itemsSold[itemNum]);
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo["money"] -= curEvent.itemsSold[itemNum].itemValue;
        buyShop.SetActive(true);
        hideMenuButtons(buyDontButtons);
    }



    private void dontPurchase()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        dontPurchaseRpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void setUpSellRpc(int itemNum, int inventoryNum)
    {
        Debug.Log("SELLING BUTTONS SHOULD BE ACTIVE");
        sellShop.SetActive(false);
        sellDontButtons[0].SetActive(true);
        sellDontButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Sell";
        var button = sellDontButtons[0].GetComponent<Button>();
        button.Select();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { sellItem(itemNum, inventoryNum);  });

        sellDontButtons[1].SetActive(true);
        var button2 = sellDontButtons[1].GetComponent<Button>();
        button2.onClick.RemoveAllListeners();
        button2.onClick.AddListener(delegate { dontSell(); });
    }


    private void sellItem(int itemNum, int inventoryNum)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        sellItemRpc(itemNum, inventoryNum);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void sellItemRpc(int itemNum, int inventoryNum)
    {
        hideMenuButtons(sellDontButtons);
        sellShop.SetActive(true);
        
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo["money"] += NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][inventoryNum].getItem(itemNum).itemValue / 2;
        NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][inventoryNum].RemoveItem(itemNum);
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

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void returnToMainRpc()
    {
        hideMenuButtons(mainMenuButtons, true);
        hideMenuButtons(buyDontButtons);
        buyShop.SetActive(false);
        sellShop.SetActive(false);
    }
}
