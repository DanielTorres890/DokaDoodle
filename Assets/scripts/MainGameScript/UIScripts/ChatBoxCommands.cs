using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatBoxCommands : MonoBehaviour
{
    public Transform textArea;
    public GameObject textGameObject;

    [TextArea(3,6)]
    public string helpText;

    [TextArea(1,2)]
    public string unknownCommand = "Unknown Command";

    [TextArea(3, 6)]
    public string helpSaveText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!NetworkManager.Singleton.IsHost) { gameObject.SetActive(false); }

    }

    
    public void ReceiveInput(string textInput)
    {
        
        if (textInput.Length <= 0) { return; }

        CreateText("\n" + textInput);
        if (textInput[0] != '/') 
        {
            UnknownCommand();
            return;
        }
        if (textInput.Length <= 4) 
        {
            UnknownCommand();
            return;
        }

        if (textInput[..5] == "/help")
        {
            CreateText(helpText);
          
        }
        else if (textInput[..5] == "/save")
        {
            if(textInput.Length <= 6)
            {
                CreateText(helpSaveText);
              
            }

            else if (int.TryParse(char.ToString(textInput[6]), out int value))
            {
                if(value <= 0 || value > 5)
                {
                    CreateText(helpSaveText);
                    
                }
                else
                {
                    DataPersistenceManager.instance.SaveGame(value - 1);
                    CreateText("Successfully saved File!");
                }
                    
            }
            else
            {
                CreateText(helpSaveText);
            }

        }

        else
        {
            UnknownCommand();
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(textArea as RectTransform);


    }


    private void CreateText(string text)
    {
        var textObj = Instantiate(textGameObject, textArea);
        textObj.GetComponent<TextMeshProUGUI>().text = text;
    }

    private void UnknownCommand()
    {
        CreateText(unknownCommand);
        CreateText(helpText);
        LayoutRebuilder.ForceRebuildLayoutImmediate(textArea as RectTransform);
    }
}
