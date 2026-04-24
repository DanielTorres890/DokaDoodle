using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class DelayedActive : AbilityBase
{
    public float prewarmDuration;
    private float prewarmTimer;

    public Collider[] myColliders;
    public UnityEvent onActive;
    private bool activated = false;
    public override void Update()
    {
        if (!IsServer) { return; }

        prewarmTimer += Time.deltaTime;
        if(prewarmDuration < prewarmTimer && !activated)
        {
            int soundId = -1;
            if (NetworkData.Instance.audioDataBase.items.Contains((attackInfo as PlacedAttack).activatedSound))
            {
                soundId = NetworkData.Instance.audioDataBase.GetId[(attackInfo as PlacedAttack).activatedSound];
            }
            
            ActivateRpc(soundId);
            activated = true;
        }
        base.Update();
    }
    public override void OnTriggerEnter(Collider other)
    {

        if (!IsServer) { return; }
        if (!activated) { return; }
        Debug.Log("Did something already enter? ");
        base.OnTriggerEnter(other);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Owner)]
    private void ActivateRpc(int soundId)
    {
        onActive.Invoke();
        if (soundId != -1)
        {
            AudioSource.PlayClipAtPoint(NetworkData.Instance.audioDataBase.GetItem[soundId], transform.position, SettingsManager.instance.SFXVolume);
        }
        

    }
    public void OnStrike()
    {
        if (hitGameObject)
        {
            var fx = Instantiate(hitGameObject);
            fx.transform.position = transform.position;
            fx.transform.localScale = transform.localScale;
           
        }
        foreach (var collider in myColliders)
        {
            collider.enabled = true;
        }
    }
    public override void OnHit()
    {

        foreach (var collider in myColliders)
        {
            collider.enabled = false;
        }
    }
}