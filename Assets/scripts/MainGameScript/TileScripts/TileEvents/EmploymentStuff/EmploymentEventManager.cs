using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class EmploymentEventManager : NetworkBehaviour
{
    public RenderTexture[] playerTextures;
    public RawImage playerDisplay;
    public static EmploymentEventManager instance;

    public GameObject UIParent;
    public GameObject confirmLeave;

    public GameObject JobChangeObject;
    public GameObject PartyMemberPurchase;
    public GameObject MainMenu;
    public GameObject confirmAllyBuy;


    public List<PartyMember> availableAllies = new List<PartyMember>();

    private List<FixedString32Bytes> randomNames = new List<FixedString32Bytes>()
    {
        "Aaliyah", "Aaron", "Abigail", "Adrian", "Aiden", "Alexander", "Amelia", 
        "Andrew", "Angel", "Anna", "Anthony", "Aria", "Asher", "Ashley", "Aubrey",
        "Austin", "Ava", "Axel", "Bella", "Benjamin", "Bennett", "Brooks", "Caleb", 
        "Camila", "Cameron", "Caroline", "Carson", "Carter", "Charlotte", "Chloe", 
        "Christian", "Christopher", "Claire", "Colton", "Connor", "Cooper", "Daniel", 
        "David", "Delilah", "Dylan", "Easton", "Eleanor", "Eli", "Elijah", "Elizabeth", 
        "Ella", "Emily", "Emma", "Ethan", "Eva", "Everett", "Ezra", 
        "Ezekiel", "Gabriel", "Genesis", "Gianna", "Grace", "Grayson", "Greyson", 
        "Hannah", "Harper", "Hazel", "Henry", "Hudson", "Hunter", "Ian", "Isaac", 
        "Isabella", "Isaiah", "Ivy", "Jack", "Jackson", "Jacob", "James", "Jameson", 
        "Jaxon", "Jayden", "Jeremiah", "John", "Jonathan", "Jordan", "Jose", "Joseph", 
        "Josiah", "Joshua", "Julia", "Julian", "Kai", "Landon", "Layla", "Leah", "Leo", 
        "Leonardo", "Levi", "Liam", "Lillian", "Lily", "Lincoln", "Logan", "Luca", "Lucas", 
        "Lucy", "Luke", "Madison", "Mason", "Mateo", "Maverick", "Mia", "Michael", 
        "Mila", "Miles", "Natalie", "Nathan", "Nicholas", "Noah", "Nolan", "Nora", "Nova", 
        "Olivia", "Oliver", "Owen", "Paisley", "Parker", "Penelope", "Pat", "Robert", "Roman", 
        "Ruby", "Ryan", "Samantha", "Samuel", "Santiago", "Scarlett", "Sebastian", "Silas", 
        "Sofia", "Sophia", "Stella", "Theodore", "Thomas", "Violet", "Waylon", "Wesley", "Weston", 
        "William", "Willow", "Wyatt", "Zoe"
    };
    public SpriteLibraryAsset spriteLibrary;
    private int numOfAllies = 5;
    private int currentAllyBuy;
    public AllyDisplay allyDisplay;
    public TextMeshProUGUI displayText;
    public UIStatUpdate moneyDisplay;

    public void Awake()
    {
        instance = this;
        
    }
    public void Start()
    {
        for(int i = 0; i < NetworkData.Instance.playerSticks.Count; i++)
        {
            NetworkData.Instance.playerSticks[i].transform.position = new Vector3(50 * i, 100, 100);
        }
        
        playerDisplay.texture = playerTextures[NetworkData.Instance.currentPlayer];
        
    }

    public override void OnNetworkSpawn()
    {
        if(IsHost)
        {
            GenerateAllies();
        }
    }

    public void SetJobChangeActive(bool toBe)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SetJobChangeActiveRpc(toBe);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetJobChangeActiveRpc(bool toBe)
    {
        JobChangeObject.SetActive(toBe);
        if(toBe) { displayText.transform.parent.gameObject.SetActive(true); }
        else { displayText.transform.parent.gameObject.SetActive(false); }
    }

    public void SetPartyChangeActive(bool toBe)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SetPartyChangeActiveRpc(toBe);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetPartyChangeActiveRpc(bool toBe)
    {
        PartyMemberPurchase.SetActive(toBe);
        if(toBe)
        {
            allyDisplay.UpdateDisplay(availableAllies);
        }
        if (toBe) { displayText.transform.parent.gameObject.SetActive(true); }
        else { displayText.transform.parent.gameObject.SetActive(false); }
    }

    public void SetMainMenuChangeActive(bool toBe)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SetMainMenuChangeActiveRpc(toBe);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetMainMenuChangeActiveRpc(bool toBe)
    {
        MainMenu.SetActive(toBe);
    }

    public void ChangePlayerClass(int classId)
    {
        if(!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        ChangePlayerClassRpc(classId);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ChangePlayerClassRpc(int classId)
    {
        NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].GetComponent<characterEditor>().setClass(classId);
        NetworkData.Instance.GetCurrentPlayer().ChangeClass(NetworkData.Instance.classDataBase.GetItem[classId]);
    }

    public void LeaveButton()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        LeaveButtonRpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LeaveButtonRpc()
    {
        confirmLeave.SetActive(true);
        UIParent.SetActive(false);
        displayText.transform.parent.gameObject.SetActive(false);
    }


    public void ConfirmLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        ConfirmLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DontLeaveRpc()
    {
        confirmLeave.SetActive(false );
        displayText.transform.parent.gameObject.SetActive(true);
    }


    private void ReadyToGenerate(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsHost)
        {
            GenerateAllies();
        }
    }
    private void GenerateAllies()
    {
        
        FixedString32Bytes[] newNames = new FixedString32Bytes[numOfAllies];
        int[] faceIds = new int[numOfAllies];
        int[] allyHair = new int[numOfAllies];
        int[] classId = new int[numOfAllies];

        int[] randomStatBoost1 = new int[numOfAllies];
        int[] randomStatBoost2 = new int[numOfAllies];


        int attributesLength = 7;
        for (int i = 0; i < numOfAllies; i++)
        {
            newNames[i] = randomNames[Random.Range(0, randomNames.Count)];
            faceIds[i] = Random.Range(0, spriteLibrary.GetCategoryLabelNames("face").ToList().Count);
            allyHair[i] = Random.Range(0,spriteLibrary.GetCategoryLabelNames("hair").ToList().Count);
            classId[i] = Random.Range(0, 3);
            randomStatBoost1[i] = Random.Range(2, attributesLength);
            randomStatBoost2[i] = Random.Range(2, attributesLength);
        }

        GenerateAlliesRpc(newNames,faceIds, allyHair, classId, randomStatBoost1, randomStatBoost2);

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void GenerateAlliesRpc(FixedString32Bytes[] names, int[] faceIds, int[] hairId,int[] classId, int[] randoBoost1, int[] randoBoost2 )
    {
        for (int i = 0;i < numOfAllies;i++)
        {
            availableAllies.Add(new PartyMember(classId[i], names[i], faceIds[i], hairId[i]));

            availableAllies[i].stats[(Attributes)randoBoost1[i]] += 1;
            availableAllies[i].stats[(Attributes)randoBoost2[i]] += 1;

        }
        allyDisplay.CreateDisplay(availableAllies);
    }

    public void SelectAlly(int index)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SelectAllyRpc(index);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SelectAllyRpc(int index)
    {
        
        currentAllyBuy = index;
        if (CalculateAllyCost(availableAllies[currentAllyBuy]) > NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.money])
        {
            displayText.text = "You can't afford this mf ";
            return;
        }
        confirmAllyBuy.SetActive(true);
        PartyMemberPurchase.SetActive(false);

    }


    public void DontBuyAlly()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        DontBuyAllyRpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DontBuyAllyRpc()
    {
        confirmAllyBuy.SetActive(false);
        PartyMemberPurchase.SetActive(true);
    }



    public void ConfirmAllyBuy()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        ConfirmAllyBuyRpc(Random.Range(0, NetworkData.Instance.classDataBase.GetItem[availableAllies[currentAllyBuy].allyClass].recommendedItems.Length));
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ConfirmAllyBuyRpc(int random)
    {

        
        var curPlayer = NetworkData.Instance.GetCurrentPlayer();

        availableAllies[currentAllyBuy].loyaltyTags = curPlayer.loyaltyTags;
        availableAllies[currentAllyBuy].curTileId = curPlayer.curTileId;
        availableAllies[currentAllyBuy].curMap = curPlayer.curMap;
        availableAllies[currentAllyBuy].allyOwner = NetworkData.Instance.currentPlayer;
        availableAllies[currentAllyBuy].AddAbility(random);

        curPlayer.partyMembers.Add(availableAllies[currentAllyBuy]);
        curPlayer.playerInfo[PlayerInfo.money] -= CalculateAllyCost(availableAllies[currentAllyBuy]);
        moneyDisplay.StatUpdate();
        availableAllies.RemoveAt(currentAllyBuy);
        allyDisplay.UpdateDisplay(availableAllies);
        confirmAllyBuy.SetActive(false);
        PartyMemberPurchase.SetActive(true);
    }

    public int CalculateAllyCost(PartyMember ally)
    {
        int totalCost = 0;
        int multiplier = 50;
        foreach(var stat in ally.stats)
        {
            if(stat.Key == Attributes.MaxHealth || stat.Key == Attributes.Health) 
                totalCost += stat.Value * multiplier / 10;    
            else
                totalCost += stat.Value * multiplier;
        }

        return totalCost;
    }
}
