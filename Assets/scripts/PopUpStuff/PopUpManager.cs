
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
        if (Instance != null) return;
        Instance = this;
    }

    

    public void PerformPopUp(int popUpId)
    {

        if (seenPopUpIds.Contains(popUpId)) { return; }
        currentPopUp = PopUpDatabase.GetItem[popUpId];
        seenPopUpIds.Add(popUpId);

        Debug.Log("Hey whats up bello");
        
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
