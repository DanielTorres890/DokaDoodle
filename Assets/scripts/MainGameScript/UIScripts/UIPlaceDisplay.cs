using TMPro;
using UnityEngine;

public class UIPlaceDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        var playerOrder = NetworkData.Instance.GetPlayerPlaceOrder();

        int PlayerPosition = 0;
        foreach(var order in playerOrder)
        {
            PlayerPosition += 1;
            if(order == NetworkData.Instance.currentPlayer)
            {
                break;
            }
        }
        string[] placeStrings = { "st", "nd", "rd", "th" };
        text.text = PlayerPosition.ToString() + placeStrings[PlayerPosition - 1];
    }
}
