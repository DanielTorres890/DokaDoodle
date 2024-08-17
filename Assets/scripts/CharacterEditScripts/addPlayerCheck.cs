using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class addPlayerCheck : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Button Button;
    [SerializeField] private int playerNum;
    private float timer = 60f;

    private void Start()
    {
        Button.onClick.AddListener(() => NetworkData.Instance.fillPlayerServerRpc(playerNum));
        
    }

    // Update is called once per frame
    private void Update()
    {
        
        if(!NetworkData.Instance.IsHost) { return; }

        if (timer > 0 )
        {
            if( NetworkData.Instance.playerCount < playerNum && (!Button.gameObject.activeSelf))
            {
                Button.gameObject.SetActive(true);
                timer = 60;
            }
            else if (NetworkData.Instance.playerCount >= playerNum && Button.gameObject.activeSelf)
            {
                Button.gameObject.SetActive(false);
                timer = 60;
            }
            
        }
        timer -= Time.deltaTime;
    }
}
