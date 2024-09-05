using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BattleUIManager : NetworkBehaviour
{
    // Start is called before the first frame update
    private GameObject fighter1;
    private GameObject fighter2;


    [SerializeField] private TextMeshProUGUI fighter1Text;
    [SerializeField] private TextMeshProUGUI fighter2Text;

    [SerializeField] private TextMeshProUGUI fighter1Name;
    [SerializeField] private TextMeshProUGUI fighter2Name;

    void Awake()
    {

        fighter1 = NetworkData.Instance.playerSticks[(PlayerCombatManager.Instance.combatant1 as playerData).playerNumber];
        fighter1.transform.position = new Vector3(-150 ,-80,200);
        fighter1.transform.localScale = new Vector3(10, 10, 1);

        if(PlayerCombatManager.Instance.combatant2 is playerData)
        {
            fighter2 = NetworkData.Instance.playerSticks[(PlayerCombatManager.Instance.combatant2 as playerData).playerNumber];
            fighter2.transform.localPosition = new Vector3(248, -146, -20);
        }
        else
        {
            var tmp = PlayerCombatManager.Instance.combatant2 as EnemyCombat;
            fighter2 = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[tmp.enemyId].enemyPrefab);
           
        }

        fighter2.transform.position = new Vector3(150, -80, 200);
        fighter2.transform.localScale = new Vector3(10, 10, 1);
        SetStatUI();
    }

    
    private void SetStatUI()
    {
        fighter1Name.text = PlayerCombatManager.Instance.combatant1.name;
        fighter2Name.text = PlayerCombatManager.Instance.combatant2.name;
        fighter1Text.text = "";
        fighter2Text.text = "";


        fighter1Text.text += "HP: " + PlayerCombatManager.Instance.combatant1.stats[Attributes.Health] + "/" + PlayerCombatManager.Instance.combatant1.stats[Attributes.MaxHealth] + "\n";
        fighter2Text.text += "HP: " + PlayerCombatManager.Instance.combatant2.stats[Attributes.Health] + "/" + PlayerCombatManager.Instance.combatant2.stats[Attributes.MaxHealth] + "\n";
        foreach (var temp in PlayerCombatManager.Instance.combatant1.stats.Keys)
        {
            if(temp == Attributes.Health || temp == Attributes.MaxHealth) { continue; }
            fighter1Text.text += temp.ToString().Remove(3)+ ": " + PlayerCombatManager.Instance.combatant1.stats[temp] + "\n";
            
        }
        foreach (var temp in PlayerCombatManager.Instance.combatant2.stats.Keys)
        {
            if (temp == Attributes.Health || temp == Attributes.MaxHealth) { continue; }
            fighter2Text.text += temp.ToString().Remove(3) + ": " + PlayerCombatManager.Instance.combatant2.stats[temp] + "\n";
        }
        
    }
    private IEnumerator Delay(float time)
    {
        yield return new WaitForSecondsRealtime(time);

    }
}
