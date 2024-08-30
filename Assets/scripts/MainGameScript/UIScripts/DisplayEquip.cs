using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayEquip : MonoBehaviour
{
    public ItemType Type;
    public ItemDataBase Data;

    private Image sprite;
    private TextMeshProUGUI text;

    private void Awake()
    {
        sprite = gameObject.transform.GetComponent<Image>();
        text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].equipItems[Type] >= 0)
        {
            sprite.sprite = Data.GetItem[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].equipItems[Type]].itemSprite;
            
            text.text = Data.GetItem[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].equipItems[Type]].itemName;
        }
        else
        {
            text.text = "Nothing";
        }
    }
}
