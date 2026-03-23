using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class playerLevelUpMnger : NetworkBehaviour
{ 
    public int statsToAllocate;
    public TextMeshProUGUI[] statText;
    public TextMeshProUGUI[] levelText;
    public TextMeshProUGUI remainingStats;
    public playerData playerWhoLevel;
    public int inControl;
    [SerializeField] private bool startShown;

    public UnityEvent onFinishLevelUp;

    public Dictionary<Attributes, int> playerStatIncrease = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0},
        

    };
    private List<Attributes> playerAttributes = new List<Attributes>
    {
        Attributes.Health,
        Attributes.Attack,
        Attributes.Defense,
        Attributes.Magic,
        Attributes.MDefense,
        Attributes.Dexterity,
      
    };

    public override void OnNetworkSpawn()
    {
        if(!startShown)
        {
            gameObject.SetActive(false);
        }
    }
    public void Setup()
    {
        gameObject.SetActive(true);
        statText[0].text = NetworkData.Instance.attributeStrings[Attributes.MaxHealth] + "\n" + playerWhoLevel.stats[Attributes.MaxHealth].ToString();

        for (int i = 1; i < statText.Length; i++)
        {
            statText[i].text = NetworkData.Instance.attributeStrings[playerAttributes[i]] + "\n" + playerWhoLevel.stats[playerAttributes[i]].ToString();
        }
        remainingStats.text = statsToAllocate.ToString();
        
    }

    public void IncreaseStat(int stat)
    {
        if(!NetworkData.Instance.IsAllowed(inControl,NetworkManager.Singleton.LocalClientId)) {  return; }
        IncreaseStatRpc(stat);

    }

    [Rpc(SendTo.ClientsAndHost,InvokePermission = RpcInvokePermission.Everyone)]
    private void IncreaseStatRpc(int stat, RpcParams parm = default)
    {
        
        if (statsToAllocate <= 0) { return; }

        if (playerAttributes[stat] == Attributes.Health)
        {
            playerStatIncrease[playerAttributes[stat]] += 10;
            playerStatIncrease[Attributes.MaxHealth] += 10;
        }
        else { playerStatIncrease[playerAttributes[stat]] += 1; }

        statsToAllocate -= 1;
        remainingStats.text = statsToAllocate.ToString();
        levelText[stat].text = playerStatIncrease[playerAttributes[stat]].ToString();

    }
    public void DecreaseStat(int stat)
    {
        if (!NetworkData.Instance.IsAllowed(inControl, NetworkManager.Singleton.LocalClientId)) { return; }
        DecreaseStatRpc(stat);

       
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void DecreaseStatRpc(int stat, RpcParams parm = default)
    {

        if (playerStatIncrease[playerAttributes[stat]] <= 0) { return; }

        if (playerAttributes[stat] == Attributes.Health)
        {
            playerStatIncrease[playerAttributes[stat]] -= 10;
            playerStatIncrease[Attributes.MaxHealth] -= 10;
        }
        else { playerStatIncrease[playerAttributes[stat]] -= 1; }
        statsToAllocate += 1;
        remainingStats.text = statsToAllocate.ToString();
        levelText[stat].text = playerStatIncrease[playerAttributes[stat]].ToString();
    }

    private IEnumerator tillChange()
    {
        while (gameObject.activeSelf)
        {
            yield return null;
        }
        if(IsServer)
        {
            SceneChanger.Instance.loadClientScenesServerRpc("MainGameScene");
        }
    }

    public void EndLevel()
    {
        if (NetworkData.Instance.IsAllowed(inControl, NetworkManager.Singleton.LocalClientId) && statsToAllocate <= 0)
        StatUpRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void StatUpRpc()
    {
        foreach (var stat in playerStatIncrease.Keys)
        {
            playerWhoLevel.ChangeBaseStat(stat, playerStatIncrease[stat]);
        }
        gameObject.SetActive(false);
        if(IsServer)
        {
            onFinishLevelUp.Invoke();
            
        }
    }
}
