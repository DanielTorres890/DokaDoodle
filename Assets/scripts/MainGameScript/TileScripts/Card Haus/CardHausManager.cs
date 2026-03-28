using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardHausManager : NetworkBehaviour
{

    public GameObject confirmLeave;
    public GameObject MainMenu;
    public GameObject BetMenu;
    public GameObject CardMenu;
    public TextMeshProUGUI displayText;

    public Sprite[] cardIcons;
    public GameObject cardPrefab;
    public Transform cardsParent;

    public bool[][] cardStates;
    public int[][] cardValues;

    public ArrayWrapper[] editorValues;

    public Vector2[] guessedLocations = new Vector2[2];
    public int currentGuess;
    private int bet;

    private int xCardCount = 5;
    private int yCardCount = 4;



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
        displayText.transform.parent.gameObject.SetActive(false);
        BetMenu.SetActive(false);
        if (IsHost)
        GenerateRandomCards();
    }
    private void GenerateRandomCards()
    {

        int[] numbers = new int[xCardCount * yCardCount];

        int value = 1;
        for (int i = 0; i < numbers.Length - 1; i += 2)
        {
            numbers[i] = value;
            numbers[i + 1] = value;

            value++;

        }
        string result = string.Join(", ", numbers);
        

        int[] randomizedNums = new int[numbers.Length];
        for (int i = 0;i < numbers.Length; i++)
        {

            int randomNum = Random.Range(0, numbers.Length);
            int lockout = 0;
            while (numbers[randomNum] == 0 || lockout > 999)
            {
                randomNum += 1;
                lockout += 1;

                if (randomNum >= numbers.Length) { randomNum = 0; }
            }
            randomizedNums[i] = numbers[randomNum];
            numbers[randomNum] = 0;
            if(lockout > 999)
            {
                Debug.Log("i wouldve gone infinite ");
            }

        }

        

        ReceieveCardsRpc(randomizedNums);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReceieveCardsRpc(int[] randomizedNumbers)
    {
        editorValues = new ArrayWrapper[xCardCount];


        cardValues = new int[xCardCount][];


        //x is 0-4
        //y is 0-3
        //0,1 should be index 5
        for (int x = 0; x < xCardCount; x++)
        {
            
            cardValues[x] = new int[yCardCount];
           
            editorValues[x] = new ArrayWrapper();
            editorValues[x].array = new int[yCardCount];

            for(int y = 0; y < yCardCount; y++)
            {
                GameObject card = Instantiate(cardPrefab, cardsParent);
                card.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = cardIcons[randomizedNumbers[x + y + (y * (xCardCount - 1))] - 1];

                cardValues[x][y] = randomizedNumbers[x + y + (y * (xCardCount - 1))];
                editorValues[x].array[y] = randomizedNumbers[x + y + (y * (xCardCount - 1))];

            }

        }

    }


}



[System.Serializable]
public class ArrayWrapper
{
    public int[] array;

}

