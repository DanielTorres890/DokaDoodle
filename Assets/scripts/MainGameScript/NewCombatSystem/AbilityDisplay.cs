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


    public void SetUp() 
    {
        abilityName.text = attack.attackName;
        abilityCooldown.text = Mathf.RoundToInt(abilityManager.stateManager[attack].cooldown).ToString();
    
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
}
