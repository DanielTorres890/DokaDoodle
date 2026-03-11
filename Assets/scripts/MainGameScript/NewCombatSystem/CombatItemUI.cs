using UnityEngine;
using UnityEngine.UI;

public class CombatItemUI : MonoBehaviour
{
    public Image itemSprite;
    public AbilityManager myManager;
    
    public void SetUp()
    {
        myManager.onItemUse.AddListener(UpdateDisplay);
        UpdateDisplay();
        
    }
    public void UpdateDisplay()
    {
        if ((myManager.stats as playerData).battleSlotItemId != -1)
        {
            gameObject.SetActive(true);
            itemSprite.sprite = NetworkData.Instance.playerInventories[0][0].database.GetItem[(myManager.stats as playerData).battleSlotItemId].itemSprite;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
