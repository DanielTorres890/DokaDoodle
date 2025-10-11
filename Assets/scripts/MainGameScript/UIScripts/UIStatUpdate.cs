using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStatUpdate : MonoBehaviour
{

    [SerializeField] private Attributes attribute;
    [SerializeField] private string statName;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private void Start()
    {
        textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        ClientChecks.Instance.onItemUse.AddListener(StatUpdate);
        ClientChecks.Instance.onClassAbilityUse.AddListener(StatUpdate);
        ClientChecks.Instance.onRoundStart.AddListener(StatUpdate);
        StatUpdate();
    }
    void Update()
    {
        
    }

    public void StatUpdate()
    {
        string color = "<color=black>";
        if (attribute != Attributes.Health && attribute != Attributes.MaxHealth && NetworkData.Instance.players[NetworkData.Instance.currentPlayer].postStatusStats[attribute] > NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attribute])
            color = "<color=#1abf3a>";
        if (attribute != Attributes.Health && attribute != Attributes.MaxHealth && NetworkData.Instance.players[NetworkData.Instance.currentPlayer].postStatusStats[attribute] < NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attribute])
            color = "<color=red>";


        textMeshProUGUI.text = statName + " "+color+NetworkData.Instance.players[NetworkData.Instance.currentPlayer].postStatusStats[attribute]+"</color>";
        if(statName.Equals("LVL"))
        {
            textMeshProUGUI.text = statName + " " + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo[PlayerInfo.level];
        }
        if(statName.Equals("Money"))
        {
            textMeshProUGUI.text = "G:" + " " + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo[PlayerInfo.money];
        }
    }
}
