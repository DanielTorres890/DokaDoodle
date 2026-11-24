using TMPro;
using UnityEngine;

public class CritDisplay : MonoBehaviour
{
    public TextMeshProUGUI critText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        critText = gameObject.GetComponent<TextMeshProUGUI>();
        ClientChecks.Instance.onItemUse.AddListener(StatUpdate);
        ClientChecks.Instance.onClassAbilityUse.AddListener(StatUpdate);
        ClientChecks.Instance.onRoundStart.AddListener(StatUpdate);
        WorldEventManager.Instance.onDayChange.AddListener(StatUpdate);
        StatUpdate();
    }

    public void StatUpdate()
    {
        critText.text = "Crit Chance: " + (Mathf.Round(NetworkData.Instance.GetCurrentPlayer().CritChance() * 100) / 100 ).ToString() + "%";
    }
}
