using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyEventDisplay : MonoBehaviour
{
    public TextMeshProUGUI display;
    public TextMeshProUGUI toolTipText;
    public Image displayIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        display = GetComponent<TextMeshProUGUI>();
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
        foreach(var wevent in WorldEventManager.Instance.activeWorldEvents)
        {
            var currentEvent = WorldEventManager.Instance.worldDatabase.GetItem[wevent.eventId];
            if (currentEvent is TimedWEvent)
            {
                display.text = currentEvent.name;
                toolTipText.text = (currentEvent as TimedWEvent).eventToolTip;
                displayIcon.sprite = (currentEvent as TimedWEvent).eventIcon;
                return;
            }
        }
        display.text = "No events going on";
        toolTipText.text = "If there was an event going on its description would be here";
        displayIcon.gameObject.SetActive(false);

    }
}
