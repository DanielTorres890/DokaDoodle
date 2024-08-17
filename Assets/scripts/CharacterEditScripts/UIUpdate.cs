using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIUpdate : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private int playerNum;
    private float timer = 60f;
    private void Update()
    {
        if (timer > 0)
        {
            if (NetworkData.Instance.players[playerNum].playerName.ToString() != text.text)
            {
                setTextToName(playerNum);
                timer = 60f;
            }
        }
        timer -= Time.deltaTime;
    }
    public void setTextToName(int playerNum)
    {
        text.text = NetworkData.Instance.players[playerNum].playerName.ToString();
    }
    
}
