using UnityEngine;
using UnityEngine.UI;

public class ReadyColor : MonoBehaviour
{
    public Image image;
    public int slotNumber;

    public Sprite checkMark;
    public Sprite cross;
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

        if (NetworkData.Instance.clientOrder[slotNumber] != -1)
        {
            image.color = Color.green;
            image.sprite = checkMark;
        }
        else
        {
            image.color = Color.red;
            image.sprite = cross;
        }
    }
}
