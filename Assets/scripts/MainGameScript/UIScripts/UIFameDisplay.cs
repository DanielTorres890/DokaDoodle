using TMPro;
using UnityEngine;

public class UIFameDisplay : MonoBehaviour
{
    private TextMeshProUGUI m_TextMeshProUGUI;

    private void Start()
    {
        m_TextMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        ClientChecks.Instance.onRoundStart.AddListener(updateFame);
        updateFame();
    }

    private void updateFame()
    {
        m_TextMeshProUGUI.text = "Fame: " + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerInfo[PlayerInfo.fame].ToString();
    }
}
