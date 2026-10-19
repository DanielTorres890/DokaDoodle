using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCombatContributions : MonoBehaviour
{
    public GameObject infoPrefab;

    //nah this is fine this is a comment of self affirmation
    public RenderTexture[] playerTextures;

    void Start()
    {
        NewCombatManager.instance.onCombatEnd.AddListener(CreateDisplay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void CreateDisplay()
    {
        foreach(var combatant in NewCombatManager.instance.allCombatants)
        {
            if(combatant == null) continue;

            GameObject combatantInfo = Instantiate(infoPrefab,transform);
            TextMeshProUGUI nameText = combatantInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI damageDealtText = combatantInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI damageTakenText = combatantInfo.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            RawImage combatantIcon = combatantInfo.transform.GetChild(4).GetComponent<RawImage>();

            nameText.text = combatant.stats.name;
            damageDealtText.text = "Damage Dealt - " + NewCombatManager.instance.damageDealt[combatant.stats].ToString();
            damageTakenText.text = "Damage Taken - " + NewCombatManager.instance.damageTaken[combatant.stats].ToString();

            if (combatant.stats is playerData)
            {
                combatantIcon.texture = playerTextures[(combatant.stats as playerData).playerNumber]; 
            }
            
            if (combatant.stats is EnemyCombat && PlayerCombatManager.Instance.EnemyDataBase.GetItem[(combatant.stats as EnemyCombat).enemyId].enemyIcon)
            {
                combatantIcon.texture = PlayerCombatManager.Instance.EnemyDataBase.GetItem[(combatant.stats as EnemyCombat).enemyId].enemyIcon;
            }

            if (combatant.stats is PartyMember)
            {
                combatantIcon.texture = combatant.GetComponent<TempRenderTexture>().tempRenderTexture;
            }
        }
    }
}
