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
            if (attributeToWhom != Attributes.Health && attributeToWhom != Attributes.MaxHealth && AbilityManager.stats.postStatusStats[attributeToWhom] > AbilityManager.stats.stats[attributeToWhom])
                color = "<color=#05FF31>";
            if (attributeToWhom != Attributes.Health && attributeToWhom != Attributes.MaxHealth && AbilityManager.stats.postStatusStats[attributeToWhom] < AbilityManager.stats.stats[attributeToWhom])
                color = "<color=red>";


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


