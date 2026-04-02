using TMPro;
using UnityEngine;

public class JumpscareButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public void Start()
    {
        if (SettingsManager.instance.canJumpscare)
        {
            buttonText.text = "On";
        }
        else
        {
            buttonText.text = "Off";
        }
    }

    public void Clicked()
    {
        SettingsManager.instance.canJumpscare = !SettingsManager.instance.canJumpscare;

        if (SettingsManager.instance.canJumpscare)
        {
            buttonText.text = "On";
        }
        else
        {
            buttonText.text = "Off";
        }
    }
}
