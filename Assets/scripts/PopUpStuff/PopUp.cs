using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PopUp : MonoBehaviour
{
    public void EndPopUp()
    {
       
        if(!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkManager.Singleton.LocalClientId)) { return; }
       
        if(!SceneChanger.Instance.everyoneLoaded()) { return; }


        SceneChanger.Instance.UnloadClientScenesRpc("PopUp");
    }
}
