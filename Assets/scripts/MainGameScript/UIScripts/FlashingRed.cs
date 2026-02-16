using UnityEngine;
using UnityEngine.UI;

public class FlashingRed : MonoBehaviour
{
    public Image whiteImage;

    public float flashSpeed = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsLow())
        whiteImage.color = new Color(255, Mathf.Sin( flashSpeed * Time.time),Mathf.Sin(flashSpeed * Time.time));
    }

    private bool IsLow()
    {
        bool islow = false;
        foreach(var partymember in NetworkData.Instance.GetCurrentPlayer().partyMembers)
        {
            if ((float)partymember.stats[Attributes.Health] / partymember.stats[Attributes.MaxHealth] < .3f)
            {
                islow = true; break;
            }
        }
        return islow;
    }
}
