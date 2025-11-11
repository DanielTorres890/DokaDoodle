using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public AbilityManager abilityManager;
    [SerializeField] private GameObject abilityDisplayPrefab;
    [SerializeField] private healthbar playerHealthBar;
    [SerializeField] private energybar playerEnergyBar;
    public int X_Start;
    public int Y_Start;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    public int X_Start_stat;
    public int Y_Start_stat;

    public void SetUp()
    {
        
        int i = 0;

        playerHealthBar.manager = abilityManager;
        playerEnergyBar.manager = abilityManager;
        playerHealthBar.gameObject.SetActive(true);
        playerHealthBar.SetUp();
        playerEnergyBar.gameObject.SetActive(true);
        playerEnergyBar.SetUp();
        
        foreach (var attack in abilityManager.orderedAttacks)
        {
         
            //WHY IS THIS A THING THAT HAS TO BE DONE
            var obj = Instantiate(abilityDisplayPrefab, Vector3.zero, Quaternion.identity, transform);
            
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
            

            var display = obj.GetComponent<AbilityDisplay>();
            
            display.attack = attack;
            display.abilityManager = abilityManager;
            if (i == abilityManager.orderedAttacks.Count - 1)
            {
                display.useKeyText.text = abilityManager.actions.actions["ClassAbility"].bindings[0].ToDisplayString();
            }
            else if (i == 0)
            {
                display.useKeyText.text = abilityManager.actions.actions["M1Attack"].bindings[0].ToDisplayString();
            }
            else
            {
                display.useKeyText.text = abilityManager.actions.actions["Ability" + i.ToString()].bindings[0].ToDisplayString();
            }
            


            display.SetUp();
            i++;
        }
    }



    private Vector3 GetPosition(int i)
    {
        return new Vector3(X_Start + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_Start + (Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }
}
