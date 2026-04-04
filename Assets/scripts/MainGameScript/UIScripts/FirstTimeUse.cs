using TMPro;
using UnityEngine;

public class FirstTimeUse : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float flashSpeed = 3;
    public TutorialStates stateToCheck;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!PopUpManager.Instance.tutorialState[stateToCheck])
        {
            text.color = new Color((Mathf.Sin(flashSpeed * Time.time) + 1)/2, (Mathf.Sin(flashSpeed * Time.time) + 1)/2, 0);
        }
        else
        {
            text.color = Color.black;
        }
    }
}
