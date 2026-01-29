 using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class InventoryChangeScript : NetworkBehaviour
{
    private int currentInventory = 0;
    [SerializeField] public DisplayInventory inventoryDisplay;

    [SerializeField] private string[] inventoryNames;
    [SerializeField] private TextMeshProUGUI nameText;
    [TextArea(3,12)]
    [SerializeField] private string[] inventoryToolTips;
    [SerializeField] private TextMeshProUGUI mouseOverText;
    [SerializeField] private TextMeshProUGUI sizeText;

    public int whomsInventory;
    public int whoInControl;

    public override void OnNetworkSpawn()
    {

        roundStart();
        //client checks SHOULD already be set if not then f them kids
        ClientChecks.Instance.onRoundStart.AddListener(roundStart);
        
    }   
  
    public void ResetDisplay()
    {
        Debug.Log("This sometimes happens for 0");
        if(NetworkData.Instance.IsAllowed(whoInControl,NetworkManager.Singleton.LocalClientId))
        {
            ResetDisplayRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetDisplayRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(whoInControl, rpcstuff.Receive.SenderClientId)) { return; }

        
        inventoryDisplay.CreateDisplay(whomsInventory, currentInventory);
        if(sizeText)
        sizeText.text = inventoryDisplay.inventory.container.Count.ToString() + "/" + inventoryDisplay.inventory.MAXSIZE.ToString();
    }

    //for some god forsaken reason my button keeps forcing itself to subscribe to inventory forward which makes 0 sense
    public void InventoryForwardFrickU()
    {

        if (NetworkData.Instance.IsAllowed(whoInControl, NetworkManager.Singleton.LocalClientId))
        {
            InventoryForwardRpc();
        }
    }
    [Rpc( SendTo.ClientsAndHost,RequireOwnership = false)]
    public void InventoryForwardRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(whoInControl, rpcstuff.Receive.SenderClientId)) { return; }

        if (currentInventory >= 2) { return; }
        
        currentInventory += 1;
        if (nameText)
        {
            nameText.text = inventoryNames[currentInventory];
            mouseOverText.text = inventoryToolTips[currentInventory];
        }
        inventoryDisplay.CreateDisplay( whomsInventory, currentInventory);
        if (sizeText)
            sizeText.text = inventoryDisplay.inventory.container.Count.ToString() + "/" + inventoryDisplay.inventory.MAXSIZE.ToString();
    }
   /* [ClientRpc( RequireOwnership = false)]
    private void InventoryForwardClientRpc(int inv)
    {
        currentInventory = inv;
        inventoryDisplay.CreateDisplay(currentInventory, NetworkData.Instance.currentPlayer);
    }*/

    public void InventoryBackFrickU()
    {
        if (NetworkData.Instance.IsAllowed(whoInControl, NetworkManager.Singleton.LocalClientId))
        {
            InventoryBackRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void InventoryBackRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(whoInControl, rpcstuff.Receive.SenderClientId)) { return; }

        if (currentInventory <= 0) { return;  }

        currentInventory -= 1;
        if(nameText)
        {
            nameText.text = inventoryNames[currentInventory];
            mouseOverText.text = inventoryToolTips[currentInventory];
        }
        
        inventoryDisplay.CreateDisplay(whomsInventory, currentInventory);
        if (sizeText)
            sizeText.text = inventoryDisplay.inventory.container.Count.ToString() + "/" + inventoryDisplay.inventory.MAXSIZE.ToString();
    }
    public void currentMouseOver()
    {
        mouseOverText.text = inventoryToolTips[currentInventory];
    }
    public void roundStart()
    {
        whomsInventory = NetworkData.Instance.currentPlayer;
        whoInControl = NetworkData.Instance.currentPlayer;
    }
}
