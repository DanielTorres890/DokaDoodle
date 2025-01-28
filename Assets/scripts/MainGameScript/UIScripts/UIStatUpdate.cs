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
    }
    void Update()
    {
        StatUpdate();
    }

    private void StatUpdate()
    {
        textMeshProUGUI.text = statName + " "+NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attribute];
        if(statName.Equals("LVL"))
        {
            textMeshProUGUI.text = statName + " " + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].level;
        }
        if(statName.Equals("Money"))
        {
            textMeshProUGUI.text = "G:" + " " + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].money;
        }
    }
}
