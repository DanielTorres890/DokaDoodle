using TMPro;
using UnityEngine;

public class DisplayTurn : MonoBehaviour
{
    private TextMeshProUGUI InControlText;
    private void Start()
    {
        InControlText = GetComponent<TextMeshProUGUI>();
        SetTurnText();
        ClientChecks.Instance.onRoundStart.AddListener(SetTurnText);
    }
    private void SetTurnText()
    {
        InControlText.text = NetworkData.Instance.GetCurrentPlayer().name + " is currently in control" ;
    }
}
