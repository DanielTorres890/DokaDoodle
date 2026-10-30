using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ReadyPlayerDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int playerNum;
    private float timer = 60f;
    public void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    public void OnEnable()
    {
        if(!NetworkManager.Singleton.IsHost) { gameObject.SetActive(false); }
    }
    private void Update()
    {
        if (playerNum > NetworkData.Instance.playerCount)
        {
            text.text = "Not Connected";
            return;
        }
        if (timer > 0)
        {


            setTextToReady(playerNum);
            timer = 60f;

        }
        timer -= Time.deltaTime;
    }
    public void setTextToReady(int playerNum)
    {
       
        if(NetworkData.Instance.readyPlayers[playerNum])
        {
            text.text = "<color=Green>Ready</color>";
        }
        else
        {
            text.text = "<color=Red>Waiting</color>";
        }
        
    }
}
