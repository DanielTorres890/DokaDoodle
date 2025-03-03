using UnityEngine;

public class StatUIDisplay : MonoBehaviour
{
    public AbilityManager abilityManager;
    [SerializeField] private GameObject statDisplayPrefab;

    public int X_Start;
    public int Y_Start;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;


    public void SetUp()
    {

        int i = 0;


        foreach (var stat in abilityManager.stats.stats.Keys)
        {
            if(stat == Attributes.Health || stat == Attributes.MaxHealth) { continue; }

            //WHY IS THIS A THING THAT HAS TO BE DONE
            var obj = Instantiate(statDisplayPrefab, Vector3.zero, Quaternion.identity, transform);

            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);


            var display = obj.GetComponent<EntityUIUpdate>();
            display.AbilityManager = abilityManager;
            display.attributeToWhom = stat;    
            display.UpdateText();
            abilityManager.onStatus.AddListener(display.UpdateText);

            i++;
        }
    }



    private Vector3 GetPosition(int i)
    {
        return new Vector3(X_Start + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_Start + (Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }
}
