using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CardHausManager : NetworkBehaviour
{

    public GameObject confirmLeave;
    public GameObject MainMenu;
    public GameObject BetMenu;
    public GameObject CardMenu;
    public TextMeshProUGUI displayText;


    public bool[][] cardStates;
    public int[][] cardValues;

    public Vector2[] guessedLocations = new Vector2[2];
    public int currentGuess;
    private int bet;


    public void SetMainMenuChangeActive(bool toBe)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SetMainMenuChangeActiveRpc(toBe);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetMainMenuChangeActiveRpc(bool toBe)
    {
        MainMenu.SetActive(toBe);
    }

    public void LeaveButton()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        LeaveButtonRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void LeaveButtonRpc()
    {
        confirmLeave.SetActive(true);
        MainMenu.SetActive(false);
        displayText.transform.parent.gameObject.SetActive(false);
    }


    public void SetBetMenuChangeActive(bool toBe)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SetBetMenuChangeActiveRpc(toBe);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetBetMenuChangeActiveRpc(bool toBe)
    {
        BetMenu.SetActive(toBe);
    }

    public void ConfirmLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        ConfirmLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ConfirmLeaveRpc()
    {
        confirmLeave.SetActive(false);
        TileEventManager.Instance.EndEvent();
    }


    public void DontLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }

        DontLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void DontLeaveRpc()
    {
        confirmLeave.SetActive(false);
        displayText.transform.parent.gameObject.SetActive(true);
    }

    public void PickBet(int amount)
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        PickBetRpc(amount);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickBetRpc(int amount)
    {
        bet = amount;
    }
    private void GenerateRandomCards()
    {

    }

}
