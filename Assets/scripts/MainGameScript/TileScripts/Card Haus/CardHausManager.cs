using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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
    public TextMeshProUGUI remainingGuessText;


    public bool[][] cardStates;
    public int[][] cardValues;
    public GameObject[][] cardObjects;

    public ItemBase[] bonusRewardsT1;
    public ItemBase[] bonusRewardsT2;
    public ItemBase[] bonusRewardsT3;


    public ArrayWrapper[] editorValues;

    public Vector2Int[] guessedLocations = new Vector2Int[2];
    public int currentGuess;
    public int remainingGuesses;

    public int[] betAmounts;
    public TextMeshProUGUI[] betTexts;

    private int totalGuesses;
    private int bet;
    private List<string> endDialogue;


    private int rewardTier;
    private int xCardCount = 5;
    private int yCardCount = 4;


    public void Start()
    {
        totalGuesses = remainingGuesses;

        for(int i = 0; i < betAmounts.Length; i++)
        {
            if (betAmounts[i] > NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.money])
            {
                betTexts[i].color = Color.red;
            }
        }

    }
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

    public void PickBet(int index)
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        if (NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.money] < betAmounts[index]) { return; }

        PickBetRpc(index);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickBetRpc(int index)
    {
        remainingGuessText.text = "Guesses: " + remainingGuesses.ToString();
        bet = betAmounts[index];
        displayText.transform.parent.gameObject.SetActive(false);
        BetMenu.SetActive(false);
        CardMenu.SetActive(true);
        NetworkData.Instance.GetCurrentPlayer().GainMoney( -betAmounts[index]);
        if (IsHost)
        GenerateRandomCards();
    }
    public void BonusTier(int tier)
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        BonusTierRpc(tier);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void BonusTierRpc(int tier)
    {
        rewardTier = tier;
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
        cardStates = new bool[xCardCount][];
        cardObjects = new GameObject[xCardCount][];

        //x is 0-4
        //y is 0-3
        //0,1 should be index 5
        for (int x = 0; x < xCardCount; x++)
        {
            
            cardValues[x] = new int[yCardCount];
            cardStates[x] = new bool[yCardCount];
            cardObjects[x] = new GameObject[yCardCount];

            editorValues[x] = new ArrayWrapper();
            editorValues[x].array = new int[yCardCount];

            for(int y = 0; y < yCardCount; y++)
            {

                int first = x;
                int second = y;
                cardStates[x][y] = false;
                
                
                GameObject card = Instantiate(cardPrefab, cardsParent);
                card.GetComponent<Button>().onClick.AddListener(delegate { PickCard(first, second); });

                card.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = cardIcons[randomizedNumbers[x + y + (y * (xCardCount - 1))] - 1];
                cardObjects[x][y] = card;


                cardValues[x][y] = randomizedNumbers[x + y + (y * (xCardCount - 1))];
                editorValues[x].array[y] = randomizedNumbers[x + y + (y * (xCardCount - 1))];

            }

        }

    }

    private void PickCard(int x, int y)
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        if(remainingGuesses <  0) { return; }
        if (cardStates[x][y]) { return; }

        if(currentGuess == 0)
        {
            if(guessedLocations[1].x == x && guessedLocations[1].y == y) { return; }
        }
        else
        {
            if (guessedLocations[0].x == x && guessedLocations[0].y == y) { return; }
        }

            PickCardRpc(x, y);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickCardRpc(int x, int y)
    {
        guessedLocations[currentGuess] = new Vector2Int(x, y);
        currentGuess++;
        flipCard(cardObjects[x][y], Quaternion.Euler(0, 180, 0));


        if(currentGuess >= 2)
        {
            currentGuess = 0;
            if (cardValues[guessedLocations[0].x][guessedLocations[0].y] == cardValues[guessedLocations[1].x][guessedLocations[1].y])
            {
                cardStates[guessedLocations[0].x][guessedLocations[0].y] = true;
                cardStates[guessedLocations[1].x][guessedLocations[1].y] = true;

                if (checkWin())
                {
                    Tween.Delay(duration: 0.5f, () => Win());
                }
            }
            else
            {
                remainingGuesses -= 1;
                int lastx = guessedLocations[0].x;
                int lasty = guessedLocations[0].y;
                Tween.Delay(duration:0.5f, () => flipCard(cardObjects[x][y], Quaternion.Euler(0,0,0)));
                Tween.Delay(duration:0.5f, () => flipCard(cardObjects[lastx][lasty], Quaternion.Euler(0, 0, 0)));
                if(remainingGuesses >= 0)
                remainingGuessText.text = "Guesses: " + remainingGuesses.ToString();

                bool lose = remainingGuesses < 0;
                if(lose)
                {
                    Tween.Delay(duration: 0.5f, () => Lose());
                }
            }
            guessedLocations[0] = new Vector2Int(-1,-1);
            guessedLocations[1] = new Vector2Int(-1, -1);
        }

    }


    private void Win()
    {
        CardMenu.SetActive(false);
        bool dropItems = DetermineRewards();
        if (dropItems)
        {
            return;
        }
        TileEventManager.Instance.EndEvent();
        

    }
    private void Lose()
    {
        CardMenu.SetActive(false);
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines.Add("Dang you lost, better luck next time ");
        TileEventManager.Instance.EndEvent();
    }
    
    private bool checkWin()
    {
        
        foreach (var arr in cardStates)
        {
            foreach(var state in arr)
            {
                if(!state)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void flipCard(GameObject card, Quaternion rotation)
    {
        Debug.Log("whoms I flipping " + card);
        Tween.Rotation(card.transform, endValue: rotation, duration: 0.5f);
    }

    private bool DetermineRewards()
    {
        List<string> spoilsDetails = new();
        NetworkData.Instance.GetCurrentPlayer().GainMoney(bet * 2);
        spoilsDetails.Add("You won " + (bet * 2).ToString() + " money congrats");
        
        if (remainingGuesses == 0) 
        {
            TileEventManager.Instance.dialogueScript.lines = spoilsDetails;
            return false;
        }

        ItemBase[] bonusRewards = bonusRewardsT1;
        if(rewardTier > 0)
        {
            bonusRewards = bonusRewardsT2;
        }
        if(rewardTier >= 2)
        {
            bonusRewards = bonusRewardsT3;
        }



        int itemIndex = Mathf.CeilToInt(remainingGuesses / ((float)totalGuesses/bonusRewards.Length)) - 1;
        string[] possibleEnds = new string[] {"good!", "extra good", "GREAT", "AMAZING ", "PERFECT"};

        bool shouldDrop = NetworkData.Instance.AddItemToInventory(NetworkData.Instance.currentPlayer, bonusRewards[itemIndex]);
        
        spoilsDetails.Add("You also won a " + bonusRewards[itemIndex].itemName + " as a bonus for doing " + possibleEnds[itemIndex]);
        TileEventManager.Instance.dialogueScript.lines = spoilsDetails;
        
        if(shouldDrop)
        {

            TileEventManager.Instance.dialogueScript.endEvent.AddListener(delegate { LoseItemManager.instance.SetUp(NetworkData.Instance.currentPlayer, bonusRewards[itemIndex].determineType()); });
            TileEventManager.Instance.dialogueScript.gameObject.SetActive(true);
            TileEventManager.Instance.dialogueScript.startDialogue();

            
            LoseItemManager.instance.finishLose.AddListener(finishLose);
        }
        
        return shouldDrop;
    }
    
    private void finishLose()
    {
        TileEventManager.Instance.dialogueScript.endEvent.RemoveAllListeners();
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines.Add("Congrats on winning! ");
        TileEventManager.Instance.EndEvent();
    }
}



[System.Serializable]
public class ArrayWrapper
{
    public int[] array;

}


