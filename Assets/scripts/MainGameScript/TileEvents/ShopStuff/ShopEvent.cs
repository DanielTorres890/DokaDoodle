using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event Object", menuName = "Events/Shop")]
public class ShopEvent : EventBase
{
    public ItemBase[] itemsSold;
    public GameObject shop;
    public List<string> endShopDialogue;
    public override void FireEvent()
    {
        var bg  = FindAnyObjectByType<Canvas>();
        var temp = Instantiate(shop,bg.transform);
        temp.GetComponentInChildren<NetworkObject>().Spawn();
        
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
