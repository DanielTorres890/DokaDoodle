using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainQuestDisplay : MonoBehaviour
{
    public TextMeshProUGUI display;
    public TextMeshProUGUI toolTipText;
    public Image displayIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        WorldEventManager.Instance.onDayChange.AddListener(UpdateDisplay);
        ClientChecks.Instance.onRoundStart.AddListener(UpdateDisplay);

    }
    private void OnEnable()
    {
        UpdateDisplay();
    }
    // Update is called once per frame
    private void UpdateDisplay()
    {
        foreach (var wevent in WorldEventManager.Instance.activeWorldEvents)
        {
            var currentEvent = WorldEventManager.Instance.worldDatabase.GetItem[wevent.eventId];
            if (WorldEventManager.Instance.questEvents.Contains(currentEvent))
            {
                display.text = currentEvent.name;
                toolTipText.text = currentEvent.eventToolTip;
                displayIcon.sprite = currentEvent.eventIcon;
                displayIcon.gameObject.SetActive(true);
                gameObject.SetActive(true);
                return;
            }
        }

        gameObject.SetActive(false);
        display.text = "No events going on";
        toolTipText.text = "If there was an event going on its description would be here";
        displayIcon.gameObject.SetActive(false);

    }
}
