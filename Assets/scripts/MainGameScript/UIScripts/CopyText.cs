using TMPro;
using UnityEngine;

public class CopyText : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = text.text;
    }
}
