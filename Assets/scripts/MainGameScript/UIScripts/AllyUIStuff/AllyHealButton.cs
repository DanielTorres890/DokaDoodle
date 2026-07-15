using TMPro;
using UnityEngine;

public class AllyHealButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;

    public void Awake()
    {
        
    }

    public void OnEnable()
    {
        SetColor();
    }

    public void SetColor()
    {
        if (NetworkData.Instance.ContainsHealingItem(NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][0]))
        {
            buttonText.color = Color.white;
        }
        else
        {
            buttonText.color = Color.red;
        }


    }
}
