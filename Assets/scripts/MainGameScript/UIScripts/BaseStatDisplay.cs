using TMPro;
using UnityEngine;

public class BaseStatDisplay : MonoBehaviour
{
    [SerializeField] private Attributes attribute;
    [SerializeField] private string statName;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;


    private void Awake()
    {
        textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        ClientChecks.Instance.onItemUse.AddListener(StatUpdate);
        ClientChecks.Instance.onClassAbilityUse.AddListener(StatUpdate);
        ClientChecks.Instance.onRoundStart.AddListener(StatUpdate);
        WorldEventManager.Instance.onDayChange.AddListener(StatUpdate);
        
        StatUpdate();
    }
    void Update()
    {

    }

    public void OnEnable()
    {
        StatUpdate();
    }
    public void StatUpdate()
    {

        string color = "<color=black>";

        if(NetworkData.Instance.GetCurrentPlayer().equipItems[ItemType.Equipment] != -1)
        {
            foreach (var buff in NetworkData.Instance.playerInventories[0][3].database.GetItem[NetworkData.Instance.GetCurrentPlayer().equipItems[ItemType.Equipment]].buffs)
            {
                if (buff.attribute == attribute)
                {
                    textMeshProUGUI.text = statName + " " + color + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attribute] + "(" + buff.value + ")" + "</color>";
                    return;
                }
            }
        }
        

        textMeshProUGUI.text = statName + " " + color + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attribute] + "(0)" + "</color>";

    }
}
