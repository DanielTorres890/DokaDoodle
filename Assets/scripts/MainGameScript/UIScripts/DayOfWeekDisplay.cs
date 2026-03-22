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
        dayText.text = WorldEventManager.Instance.daysOfTheWeek[WorldEventManager.Instance.days];
    }
}
