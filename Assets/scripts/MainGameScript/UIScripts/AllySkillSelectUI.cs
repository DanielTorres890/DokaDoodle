

using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AllySkillSelectUI : NetworkBehaviour
{

    public GameObject allySkillDisplay;
    public GameObject confirmButtons;

    public GameObject skillButtonPrefab;
    [SerializeField]private List<GameObject> spawnedPrefabs;


    public TextMeshProUGUI partyMemberName;

    public UnityEvent onFinished;

    private int[] possibleSkills;
    private int selectedIndex;
    private int inControl;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(false);

    }
    public void SetUp(int playerId)
    {

        if (!IsHost) { return; }

        PartyMember selectedMember = PartyMemberPick(playerId);
        if (selectedMember == null) { return; }

        int[] skillSelection = { -1, -1, -1};

        var selectedClassItemsRec = NetworkData.Instance.classDataBase.GetItem[selectedMember.allyClass].recommendedItems;
        for (int i = 0; i < skillSelection.Length; i++)
        {
            int rando = Random.Range(0, selectedClassItemsRec.Length);
            int itemType = selectedClassItemsRec[rando].determineType();

            int itemId = NetworkData.Instance.playerInventories[0][itemType].database.GetId[selectedClassItemsRec[rando]];

            int cycleLimit = 0;
            Debug.Log("Initial Roll " + rando);
            while (((selectedMember.weaponsInventory.Contains(itemId) && itemType == 1) || (selectedMember.magicInventory.Contains(itemId) && itemType == 2) || skillSelection.Contains(rando)) && cycleLimit <= selectedClassItemsRec.Length)
            {
                Debug.Log("Move to next roll " + cycleLimit);
                rando = (rando + 1) % selectedClassItemsRec.Length;
                itemType = selectedClassItemsRec[rando].determineType();
                itemId = NetworkData.Instance.playerInventories[0][itemType].database.GetId[selectedClassItemsRec[rando]];
                cycleLimit += 1;
                
            }
            if(cycleLimit > selectedClassItemsRec.Length)
            {
                Debug.Log("We hit the limit?");
                rando = -1;
                cycleLimit = 0;
            }
            skillSelection[i] = rando;
          
        }
        SetUpRpc(playerId, skillSelection);

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetUpRpc(int playerId, int[] skillSelection)
    {

        confirmButtons.SetActive(false);
        allySkillDisplay.transform.parent.gameObject.SetActive(true);
        foreach (var button in spawnedPrefabs)
        {
            Destroy(button);
        }
        spawnedPrefabs.Clear();


        inControl = playerId;
        gameObject.SetActive(true);
        possibleSkills = skillSelection;

        
        PartyMember memberToLevel = PartyMemberPick(playerId);
        PlayerClassBase allyClass = NetworkData.Instance.classDataBase.GetItem[memberToLevel.allyClass];

        partyMemberName.text = memberToLevel.name;
        bool onlyNegative = true;
        for(int i = 0; i < skillSelection.Length; i++)
        {
            Debug.Log("What skills are selected " + skillSelection[i]);
            if (skillSelection[i] == -1) { continue; }

            onlyNegative = false;
            GameObject skillButton = Instantiate(skillButtonPrefab, allySkillDisplay.transform);
            skillButton.GetComponent<AllySkillButton>().SetUp(allyClass.recommendedItems[skillSelection[i]]);

            int index = i;

            skillButton.GetComponent<Button>().onClick.AddListener(delegate { SelectSkill(index); });
            spawnedPrefabs.Add(skillButton);

        }

        if (onlyNegative)
        {
            gameObject.SetActive(false);
            onFinished.Invoke();
        }
    }
    private PartyMember PartyMemberPick(int playerId)
    {
        for (int i = 0; i < NetworkData.Instance.players[playerId].partyMembers.Count; i++)
        {
            if (NetworkData.Instance.players[playerId].partyMembers[i].skillsToGain > 0) {  return NetworkData.Instance.players[playerId].partyMembers[i]; }
        }
        return null;
    }
    private void SelectSkill(int skillIndex)
    {
        if(!NetworkData.Instance.IsAllowed(inControl, NetworkManager.Singleton.LocalClientId)) { return; }

        SelectSkillRpc(skillIndex);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SelectSkillRpc(int skillIndex)
    {
        selectedIndex = skillIndex;
        confirmButtons.SetActive(true);
        allySkillDisplay.transform.parent.gameObject.SetActive(false);

    }

    public void ConfirmSkill()
    {
        if (!NetworkData.Instance.IsAllowed(inControl, NetworkManager.Singleton.LocalClientId)) { return; }
        ConfirmSkillRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ConfirmSkillRpc()
    {
        PartyMember memberToLevel = PartyMemberPick(inControl);
        memberToLevel.AddAbility(possibleSkills[selectedIndex]);
        gameObject.SetActive(false);
        
        memberToLevel = PartyMemberPick(inControl);
        if(memberToLevel == null) { onFinished.Invoke(); }
        else { SetUp(inControl); }
        
        
    }

    public void GoBack()
    {
        if (!NetworkData.Instance.IsAllowed(inControl, NetworkManager.Singleton.LocalClientId)) { return; }
        GoBackRpc();

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void GoBackRpc()
    {
        confirmButtons.SetActive(false);
        allySkillDisplay.transform.parent.gameObject.SetActive(true);

    }

}
