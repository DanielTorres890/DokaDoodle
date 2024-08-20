using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class addPlayerCheck : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Button Button;
    [SerializeField] private int playerNum;
   
    private void Start()
    {
        Button.onClick.AddListener(() => NetworkData.Instance.fillPlayerServerRpc(playerNum));
        
    }

    // Update is called once per frame
    private void Update()
    {
        
        if(!NetworkData.Instance.IsHost) { return; }

      
            if( NetworkData.Instance.playerCount < playerNum && (!Button.gameObject.activeSelf))
            {
                Button.gameObject.SetActive(true);
             
            }
            else if (NetworkData.Instance.playerCount >= playerNum && Button.gameObject.activeSelf)
            {
                Button.gameObject.SetActive(false);
               
            }
       
    }
}
