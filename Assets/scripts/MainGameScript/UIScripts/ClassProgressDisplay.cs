using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassProgressDisplay : MonoBehaviour
{
    public Image[] stars;
    public TextMeshProUGUI className;
    public int currentClass;
    public TextMeshProUGUI itemInvSize;
    public TextMeshProUGUI physInvSize;
    public TextMeshProUGUI magicInvSize;
    public void OnEnable()
    {
        UpdateClassDisplay(NetworkData.Instance.GetCurrentPlayer().playerClass);
    }

    public void UpdateClassDisplay(int lookedClass)
    {
        var currentPlayer = NetworkData.Instance.GetCurrentPlayer();
        currentClass = lookedClass;


        for(int i = 0; i < stars.Length; i++)
        {
            if (currentPlayer.playerClassProgress[currentClass].level > i)
            {
                stars[i].color = Color.gold;
            }
            else
            {
                stars[i].color = Color.grey;
            }
        }
        var thisClass = NetworkData.Instance.classDataBase.GetItem[currentClass];
        className.text = thisClass.className;
        itemInvSize.text = thisClass.inventorySizes[0].ToString();
        physInvSize.text = thisClass.inventorySizes[1].ToString();
        magicInvSize.text = thisClass.inventorySizes[2].ToString();





    }
}
