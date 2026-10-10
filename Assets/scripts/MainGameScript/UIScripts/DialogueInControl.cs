using TMPro;
using UnityEngine;

public class DialogueInControl : MonoBehaviour
{
    public TextMeshProUGUI text;
    public DialogueScript dialogueBox;
    public int prevInControl;
    void Start()
    {
        prevInControl = dialogueBox.whoInControl;
        text.text = NetworkData.Instance.players[prevInControl].name + " is in control";

    }

    // Update is called once per frame
    void Update()
    {
        if(prevInControl != dialogueBox.whoInControl)
        {
            prevInControl = dialogueBox.whoInControl;
            text.text = NetworkData.Instance.players[prevInControl].name + " is in control";
        }
    }
}
