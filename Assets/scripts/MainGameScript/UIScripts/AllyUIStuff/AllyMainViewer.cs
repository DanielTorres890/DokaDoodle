using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllyMainViewer : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    public int X_Start;
    public int Y_Start;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    private List<GameObject> displayedGameObjects = new List<GameObject>();


    private List<PartyMember> currentValues;


    private void Start()
    {

    }

    public void CreateDisplay(List<PartyMember> displayedAllies)
    {
        currentValues = displayedAllies;
        //CONTINUE MF

        var classDictionary = NetworkData.Instance.GetCurrentPlayer().playerClassProgress;
        int i = 0;
        foreach (var entity in currentValues)
        {
            //This makes even less sense i IS NOT THE ITERATOR BUT SINCE IT WAS DECLARED OUTSIDE OF THE LOOP IT MEANS DELEGATES PASS THE LAST VALUE IT HAS BEFORE ITS DEALLOCATED(?)
            int yofyoungl = i;

            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);


            obj.GetComponent<Button>().onClick.AddListener(delegate {  });


            //this feels bleh but no other way to line it up nicely
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = entity.name;
            obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.classDataBase.GetItem[entity.allyClass].className;
            
            int childCounter = 2;
            foreach(var stat in entity.stats)
            {
                obj.transform.GetChild(childCounter).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.attributeStrings[stat.Key] + $" {stat.Value,-4}";
                childCounter += 1;
            }
            displayedGameObjects.Add(obj);
            i++;
        }
    }
    public void UpdateDisplay(List<PartyMember> displayedAllies)
    {
        foreach (var obj in displayedGameObjects)
        {
            Destroy(obj);
        }
        displayedGameObjects.Clear();
        CreateDisplay(displayedAllies);
    }
    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);

    }
    public Vector3 GetPosition(int i)
    {
        return new Vector3(X_Start + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_Start + (-Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }

   

}
