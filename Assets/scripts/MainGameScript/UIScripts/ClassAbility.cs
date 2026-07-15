using TMPro;
using UnityEngine;

public class ClassAbility : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI abilityDescription;

    public void Start()
    {
        
        ClientChecks.Instance.onRoundStart.AddListener(delegate { setButtonText(); });
        setButtonText();
    }
    public void setButtonText()
    {
        var classScriptable = NetworkData.Instance.classDataBase.GetItem[NetworkData.Instance.GetCurrentPlayer().playerClass];
        abilityDescription.text = classScriptable.overworldAbilityDescription;
        if (NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd] > 0) 
        {
            buttonText.text = NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd].ToString();
        }
        else
        {
            buttonText.text = classScriptable.classActionName;
        }

        if(classScriptable.actionType == ClassAbilityType.Combat)
        {
            buttonText.color = Color.blue;
        }
        else if(classScriptable.actionType == ClassAbilityType.Movement)
        {
            buttonText.color = Color.green;
        }
        else
        {
            buttonText.color = Color.yellow;
        }
    }
    public void UseClassAbility()
    {

        if ( !NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkData.Instance.NetworkManager.LocalClientId) || NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd] > 0) { return; }
        ClientChecks.Instance.UseClassAbilityRpc(Random.Range(0,1000));
    }


}
