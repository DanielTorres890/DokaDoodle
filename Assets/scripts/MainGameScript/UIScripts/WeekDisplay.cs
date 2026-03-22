using TMPro;
using UnityEngine;

public class WeekDisplay : MonoBehaviour
{
    public TextMeshProUGUI weekText;
    void Start()
    {
        weekText = GetComponent<TextMeshProUGUI>();
        UpdateText();
        WorldEventManager.Instance.onDayChange.AddListener(UpdateText);
    }

    // Update is called once per frame
    private void UpdateText()
    {
        weekText.text = "Week " +  (WorldEventManager.Instance.weeks + 1).ToString();
    }
}
