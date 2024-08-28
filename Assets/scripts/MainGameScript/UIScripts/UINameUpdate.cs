using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UINameUpdate : MonoBehaviour
{
    private TextMeshProUGUI m_TextMeshProUGUI;

    private void Start()
    {
        m_TextMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        updateName();
    }

    private void updateName()
    {
        m_TextMeshProUGUI.text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].getName().ToString();
    }
}
