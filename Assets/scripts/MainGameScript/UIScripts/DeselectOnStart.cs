using UnityEngine;
using UnityEngine.UI;

public class DeselectOnStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var button = GetComponent<Button>();
        button.deselect
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
