using System;
using TMPro;
using UnityEngine;

public class EntityUIUpdate : MonoBehaviour
{
    public AbilityManager AbilityManager;

    public string whomToUpdate;

    public TMP_Text textMeshProUGUI;

    public Attributes attributeToWhom;

    public bool faceTowards = false;
    public void UpdateText()
    {
        Debug.Log("I should've updated");
        if (whomToUpdate.ToLower() == "name")
        {
            textMeshProUGUI.text = AbilityManager.stats.name;
        }

        else if (whomToUpdate.ToLower() == "stat" && attributeToWhom == Attributes.Health)
        {
            

            textMeshProUGUI.text = AbilityManager.stats.postStatusStats[Attributes.Health] + "/" + AbilityManager.stats.postStatusStats[Attributes.MaxHealth];
        }
        else
        {
            string color = "<color=black>";
            if (attributeToWhom != Attributes.Health && attributeToWhom != Attributes.MaxHealth && NetworkData.Instance.players[NetworkData.Instance.currentPlayer].postStatusStats[attributeToWhom] > NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attributeToWhom])
                color = "<color=#1abf3a>";
            if (attributeToWhom != Attributes.Health && attributeToWhom != Attributes.MaxHealth && NetworkData.Instance.players[NetworkData.Instance.currentPlayer].postStatusStats[attributeToWhom] < NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[attributeToWhom])
                color = "<color=red";


            textMeshProUGUI.text = NetworkData.Instance.attributeStrings[attributeToWhom] + " " + color + AbilityManager.stats.postStatusStats[attributeToWhom] + "</color>";
        }
    }
    private void Update()
    {
        if(!faceTowards) { return; }
        textMeshProUGUI.gameObject.transform.LookAt(NewCombatManager.instance.cameras[NewCombatManager.instance.currentSpec].gameObject.transform.position);
        textMeshProUGUI.gameObject.transform.RotateAround(textMeshProUGUI.gameObject.transform.position, transform.up, 180f);
    }

}


