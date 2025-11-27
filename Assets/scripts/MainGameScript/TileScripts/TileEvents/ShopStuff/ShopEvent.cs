using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event Object", menuName = "Events/Shop")]
public class ShopEvent : EventBase
{
    public ItemBase[] itemsSold;
    public List<string> endShopDialogue;
    public EnemyEncounter storeDefense;
    public override void FireEvent()
    {
        //no need for any of this then if its a seperate scene now im big sad
        /*if (NetworkData.Instance.IsHost)
        {
            var bg = FindAnyObjectByType<Canvas>();
            var temp = Instantiate(shop, bg.transform);
            temp.transform.GetComponentInChildren<NetworkObject>().Spawn();//im kinda depressed ab this ngl HOW ELSE WOULD YOU MAKE SYNCED STUFF WITHOUT USING A DIFFERENT SCENE BRU(maybe a ill switch to synced scene later)
        }*/
        
        
    }

    public override void RandomPassBack()
    {
        throw new System.NotImplementedException();
    }

    public override void SetUpBg()
    {
        throw new System.NotImplementedException();
    }

   
}
