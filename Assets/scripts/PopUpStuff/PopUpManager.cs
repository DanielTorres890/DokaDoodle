
using System.Collections.Generic;
using UnityEngine;

public class PopUpManager : MonoBehaviour, IDataPersistance
{

    public static PopUpManager Instance;
    public PopUpDatabase PopUpDatabase;
    public GameObject currentPopUp;
    public List<int> seenPopUpIds;

    public void Awake()
    {
        if (Instance != null) { Destroy(this); Destroy(this.gameObject); return; }
        Instance = this;
    }

    

    public void PerformPopUp(int popUpId, bool repeating = false, bool perPlayer = false)
    {

        if (seenPopUpIds.Contains(popUpId) && !repeating) { return; }
        if(perPlayer && NetworkData.Instance.GetCurrentPlayer().seenPopups.Contains(popUpId)) { return; }

        Debug.Log("Which pop up am i zooing " +  popUpId);
        currentPopUp = PopUpDatabase.GetItem[popUpId];
        seenPopUpIds.Add(popUpId);

        if(perPlayer) { NetworkData.Instance.GetCurrentPlayer().seenPopups.Add(popUpId); }

        
        if (SceneChanger.Instance.IsServer)
        {
            SceneChanger.Instance.loadClientScenesAddidtiveRpc("PopUp");
        }
       
        
    }

    
    public void LoadData(GameData data)
    {
        seenPopUpIds = data.seenPopsUps;
    }
    public void SaveData(ref GameData data)
    {
        data.seenPopsUps = seenPopUpIds;
    }
    // Update is called once per frame
 
}
