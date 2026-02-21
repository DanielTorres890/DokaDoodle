using NUnit.Framework;
using TMPro;
using UnityEngine;

public class AbilityDisplay : MonoBehaviour
{
    public TextMeshProUGUI abilityName;
    public TextMeshProUGUI abilityCooldown;
    public TextMeshProUGUI useKeyText;
    public AttackBase attack;
    public AbilityManager abilityManager;
    public GameObject conditionGreyBox;

    public void SetUp() 
    {
        abilityName.text = attack.attackName;
        abilityCooldown.text = Mathf.RoundToInt(abilityManager.stateManager[attack].cooldown).ToString();
        DisplayConditional();
        abilityManager.onStatus.AddListener(DisplayConditional);
    }
    // Update is called once per frame
    void Update()
    {
        if (abilityManager == null ) { return; }

        if (abilityManager.stateManager[attack].cooldown <= 0)
        {
            abilityCooldown.text = "0";
            return;
        }

        abilityCooldown.text = Mathf.RoundToInt(abilityManager.stateManager[attack].cooldown).ToString();

        
    }
    
    private void DisplayConditional()
    {
        bool conditionMet = true;
        foreach(var condition in attack.conditions)
        {
            
            if(!condition.Condition(abilityManager)) { conditionMet = false; break; }
        }
        conditionGreyBox.SetActive(!conditionMet);
    }
}
