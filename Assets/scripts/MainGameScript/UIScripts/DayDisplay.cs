using TMPro;
using UnityEngine;

public class DayDisplay : MonoBehaviour
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
        dayText.text = "Day " + (WorldEventManager.Instance.days + WorldEventManager.Instance.weeks * WorldEventManager.Instance.daysPerWeek).ToString();
    }
}
