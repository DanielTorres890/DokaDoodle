using UnityEngine;
using UnityEngine.EventSystems;

public class ForceSelect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SelectThis()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
