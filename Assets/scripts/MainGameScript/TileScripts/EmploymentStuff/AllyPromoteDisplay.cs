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
    public GameObject AllyView;
    public TextMeshProUGUI unlockConditions;
    public TextMeshProUGUI allyNameText;
    private void Start()
    {

    }

    public void CreateDisplay(List<PartyMember> displayedAllies)
    {
        currentValues = displayedAllies;
        SetAllyVisuals(0);
        if (currentValues.Count <= 0) { return; }
        //CONTINUE MF
        
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

            AddEvent(obj, EventTriggerType.PointerClick, delegate { SetAllyVisuals(yofyoungl); });

            AddEvent(obj, EventTriggerType.PointerClick, delegate { CreateDisplay(yofyoungl); });
           


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
      
        if(currentValues.Count <= 0)
        {
            unlockConditions.text = "If you had any allies they would show up here ";
            AllyView.SetActive(false);
            return;
        }

        allyNameText.text = currentValues[allyIndex].name;
        AllyView.SetActive(true);
        allyEditor.setClass(currentValues[allyIndex].allyClass);
        allyEditor.setHair(currentValues[allyIndex].allyHair);
        allyEditor.setFace(currentValues[allyIndex].allyFace);

        SetAllyStatsDisplay(allyIndex);



    }
    private void SetAllyStatsDisplay(int allyIndex)
    {
        allyTextDisplay.text =
           NetworkData.Instance.attributeStrings[Attributes.Health] + ": " +
           currentValues[allyIndex].stats[Attributes.Health].ToString() + "/" +
           currentValues[allyIndex].stats[Attributes.MaxHealth].ToString() + "\n";

        foreach (var stat in currentValues[allyIndex].stats)
        {
            if (stat.Key == Attributes.Health || stat.Key == Attributes.MaxHealth) { continue; }
            allyTextDisplay.text += NetworkData.Instance.attributeStrings[stat.Key] + ": " + stat.Value.ToString() + "\n";
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


            int classId = NetworkData.Instance.classDataBase.GetId[possibleClass];
            obj.GetComponent<Button>().onClick.AddListener(delegate { EmploymentEventManager.instance.SelectPromoteAlly(allyIndex, classId); });

            //AddEvent(obj, EventTriggerType.Select, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[entity.allyClass].classDescription); });
            //AddEvent(obj, EventTriggerType.PointerEnter, delegate { displayText.SetText(NetworkData.Instance.classDataBase.GetItem[entity.allyClass].classDescription); });

            AddEvent(obj, EventTriggerType.Select, delegate { SetUnlockCondition(classId); });
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { SetUnlockCondition(classId); });

            obj.GetComponentInChildren<TextMeshProUGUI>().text = possibleClass.className;
            displayedGameObjectsPromoteOptions.Add(obj);
            i++;
        }
    }
    private void SetUnlockCondition(int classId)
    {
        string info = NetworkData.Instance.classDataBase.GetItem[classId].allyUnlockTips;
        //unlockConditions.text = "Minimum Level " + (NetworkData.Instance.classDataBase.GetItem[classId].classTier * 10).ToString();
        unlockConditions.text = NetworkData.Instance.classDataBase.GetItem[classId].className + ": " + info;
    }
    
}