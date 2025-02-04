using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShopUISync : NetworkBehaviour
{
    [SerializeField] private GameObject buyShop;
    [SerializeField] private List<GameObject> mainMenuButtons;
    [SerializeField] private List<GameObject> buyDontButtons;

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
}
