using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllyPromoteDisplay : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;

    private List<GameObject> displayedGameObjects = new List<GameObject>();
    private List<GameObject> displayedGameObjectsPromoteOptions = new List<GameObject>();
    public characterEditor allyEditor;

    private List<PartyMember> currentValues;
    public Transform classButtonParent;
    public TextMeshProUGUI allyTextDisplay;

    public TextMeshProUGUI unlockConditions;
    private void Start()
    {

    }

    public void CreateDisplay(List<PartyMember> displayedAllies)
    {
        currentValues = displayedAllies;
        if(currentValues.Count <= 0) { return; }
        //CONTINUE MF
        SetAllyVisuals(0);
        var classDictionary = NetworkData.Instance.GetCurrentPlayer().playerClassProgress;
        int i = 0;
        foreach(var spawned in displayedGameObjects)
        {
            Destroy(spawned);
        }
        displayedGameObjects.Clear();
        foreach (var entity in currentValues)
        {
            //This makes even less sense i IS NOT THE ITERATOR BUT SINCE IT WAS DECLARED OUTSIDE OF THE LOOP IT MEANS DELEGATES PASS THE LAST VALUE IT HAS BEFORE ITS DEALLOCATED(?)
            int yofyoungl = i;

            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, transform);
           


            //obj.GetComponent<Button>().onClick.AddListener(delegate { EmploymentEventManager.instance.SelectAlly(yofyoungl); });

            //AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[entity.allyClass].classDescription); });
            //AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[entity.allyClass].classDescription); });

            AddEvent(obj, EventTriggerType.Select, delegate { SetAllyVisuals(yofyoungl); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { SetAllyVisuals(yofyoungl); });

            AddEvent(obj, EventTriggerType.Select, delegate { CreateDisplay(yofyoungl); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { CreateDisplay(yofyoungl); });


            obj.GetComponentInChildren<TextMeshProUGUI>().text = entity.name;
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
   
    public void SetAllyVisuals(int allyIndex)
    {
      
        allyEditor.setClass(currentValues[allyIndex].allyClass);
        allyEditor.setHair(currentValues[allyIndex].allyHair);
        allyEditor.setFace(currentValues[allyIndex].allyFace);

    }
    private void SetAllyStatsDisplay(int allyIndex)
    {
        allyTextDisplay.text = "";
        foreach (var stat in currentValues[allyIndex].stats)
        {
            allyTextDisplay.text += NetworkData.Instance.attributeStrings[stat.Key] + ": " + stat.Value + "\n";
        }
    }

    public void CreateDisplay(int allyIndex)
    {
        var thisAlly = NetworkData.Instance.GetCurrentPlayer().partyMembers[allyIndex];
        var AllyClass = NetworkData.Instance.classDataBase.GetItem[thisAlly.allyClass];
        int i = 0;
        foreach (var spawned in displayedGameObjectsPromoteOptions)
        {
            Destroy(spawned);
        }
        displayedGameObjectsPromoteOptions.Clear();
        foreach (var possibleClass in AllyClass.allyClassUpgrades)
        {
            //This makes even less sense i IS NOT THE ITERATOR BUT SINCE IT WAS DECLARED OUTSIDE OF THE LOOP IT MEANS DELEGATES PASS THE LAST VALUE IT HAS BEFORE ITS DEALLOCATED(?)
            int yofyoungl = i;

            var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, classButtonParent);



            obj.GetComponent<Button>().onClick.AddListener(delegate { EmploymentEventManager.instance.SelectPromoteAlly(allyIndex, thisAlly.allyClass); });

            //AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[entity.allyClass].classDescription); });
            //AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[entity.allyClass].classDescription); });

            AddEvent(obj, EventTriggerType.Select, delegate { SetUnlockCondition(thisAlly.allyClass); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { SetUnlockCondition(thisAlly.allyClass); });

            obj.GetComponentInChildren<TextMeshProUGUI>().text = possibleClass.className;
            displayedGameObjectsPromoteOptions.Add(obj);
            i++;
        }
    }
    private void SetUnlockCondition(int classId)
    {
        string info = NetworkData.Instance.classDataBase.GetItem[classId].allyUnlockTips;
        unlockConditions.text = "Minimum Level " + (NetworkData.Instance.classDataBase.GetItem[classId].classTier * 10).ToString();
        unlockConditions.text += info;
    }
    
}
