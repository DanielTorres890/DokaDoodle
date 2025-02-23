using System;
using TMPro;
using UnityEngine;

public class EntityUIUpdate : MonoBehaviour
{
    public AbilityManager AbilityManager;

    public string whomToUpdate;

    public TextMeshPro textMeshProUGUI;

    public Attributes attributeToWhom;
    public void UpdateText()
    {
        if (whomToUpdate.ToLower() == "name")
        {
            textMeshProUGUI.text = AbilityManager.stats.name;
        }

        else if (whomToUpdate.ToLower() == "stat" && attributeToWhom == Attributes.Health)
        {
            textMeshProUGUI.text = AbilityManager.stats.stats[Attributes.Health] + "/" + AbilityManager.stats.stats[Attributes.MaxHealth];
        }
        else
        {
            textMeshProUGUI.text = NetworkData.Instance.attributeStrings[attributeToWhom] + " " + AbilityManager.stats.stats[attributeToWhom];
        }
    }
    private void Update()
    {
        textMeshProUGUI.gameObject.transform.LookAt(NewCombatManager.instance.cameras[NewCombatManager.instance.currentSpec].gameObject.transform.position);
        textMeshProUGUI.gameObject.transform.RotateAround(textMeshProUGUI.gameObject.transform.position, transform.up, 180f);
    }

}


