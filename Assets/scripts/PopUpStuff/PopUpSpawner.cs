using UnityEngine;

public class PopUpSpawner : MonoBehaviour
{
    public Canvas canvas;

    public void Awake()
    {
        canvas = GetComponent<Canvas>();

    }
    public void Start()
    {
        Instantiate(PopUpManager.Instance.currentPopUp,canvas.transform);
    }
}
