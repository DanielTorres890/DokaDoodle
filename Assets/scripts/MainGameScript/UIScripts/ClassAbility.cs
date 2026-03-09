using TMPro;
using UnityEngine;

public class ClassAbility : MonoBehaviour
{
    public TextMeshProUGUI buttonText;

    public void Start()
    {
        
        ClientChecks.Instance.onRoundStart.AddListener(delegate { setButtonText(); });
        setButtonText();
    }
    public void setButtonText()
    {
        if (NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd] > 0) 
        {
            buttonText.text = NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd].ToString();
        }
        else
        {
            buttonText.text = "USE ABILITY";
        }
    }
    public void UseClassAbility()
    {

        if ( !NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkData.Instance.NetworkManager.LocalClientId) || NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd] > 0) { return; }
        ClientChecks.Instance.UseClassAbilityRpc(Random.Range(0,1000));
    }


}
