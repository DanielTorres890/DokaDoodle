using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnityRelay : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text joinCodeTextLoaded;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button submitCode;
    [SerializeField] private GameObject editor;
    [SerializeField] private GameObject characterPreview;
    private async void Start()
    {
        joinCodeInput.onEndEdit.AddListener(JoinRelay);
        
        
        await UnityServices.InitializeAsync();

        if(!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        
        
    }

    public async void CreateRelay()
    {
        try
        {

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = "Join Code " + joinCode;


            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if(NetworkManager.Singleton.StartHost())
            {
                
            }
            
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            
        }
    
    }

    public async void JoinRelay(string joinCode)
    {
        try 
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);


            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            

            if(NetworkManager.Singleton.StartClient() )
            {         

                    if (NetworkData.Instance.LoadedIn)
                        characterPreview.SetActive(true);
                  

            }


            //editor.SetActive(true);
            joinCodeTextLoaded.text = "Join Code " + joinCode;
            joinCodeInput.gameObject.SetActive(false);
            joinCodeText.text = "Join Code " + joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

        }
        
    }

    //load saved game
    public async void CreateRelayLoad()
    {
        try
        {

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeTextLoaded.text ="Join Code " + joinCode;


            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (NetworkManager.Singleton.StartHost())
            {
                
            }

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

        }

    }
}
