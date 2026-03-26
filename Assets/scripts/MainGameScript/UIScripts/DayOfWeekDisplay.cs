using TMPro;
using UnityEngine;

public class DayOfWeekDisplay : MonoBehaviour
{
    public TextMeshProUGUI dayText;
    void Start()
    {
        dayText = GetComponent<TextMeshProUGUI>();
        UpdateText();
        WorldEventManager.Instance.onDayChange.AddListener(UpdateText);
    }

    // Update is called once per frame
    private void UpdateText()
    {
        if(WorldEventManager.Instance.days >= WorldEventManager.Instance.daysPerWeek)
        {
            dayText.text = WorldEventManager.Instance.daysOfTheWeek[0];
            return;
        }
        dayText.text = WorldEventManager.Instance.daysOfTheWeek[WorldEventManager.Instance.days];
    }
}
