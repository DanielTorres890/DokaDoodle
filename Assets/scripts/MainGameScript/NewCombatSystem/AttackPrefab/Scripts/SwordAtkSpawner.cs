using System.Linq;
using Unity.Netcode;
using UnityEngine;

//important to note that it spawns things in a row
public class SwordAtkSpawner : NonDamage
{
    

    [Tooltip("How big the gap between each attack spawn, if there is multiple")]
    public float attackGap = 1;
    public override void AbilityAction()
    {
        if (!IsServer) { return; }
        lifetimer += Time.deltaTime;

        int counter = 0;
        AbilityManager ownerManager = owner.GetComponent<AbilityManager>();
        
        for (int i = ownerStats.statuses.Count - 1; i >= 0; i--)
        {
            
            var status = ownerStats.statuses[i];
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == (attackInfo.conditions[0] as RequireBuff).requiredBuff)
            {
                
                counter++;
            }
        }
        if(counter <= 0) { return; }
        for (int i = 0; i < counter; i++)
        {
            ownerManager.ILostBuffRpc(NetworkData.Instance.buffDataBase.GetId[(attackInfo.conditions[0] as RequireBuff).requiredBuff]);
        }

        
        int didISpawn = 1;//im sure theres a better way but brain fried and this is path to least resistance
        if (counter % 2 == 1)
        {
            
            var meteorInfo = (attackInfo as MeteorShower);
            var mainAttack = meteorInfo.meteorInfo.WeaponEffect(owner, NetworkManager.Singleton.LocalTime.TimeAsFloat, owner.transform.position, owner.transform.eulerAngles, 1,owner.transform.position, owner.transform.eulerAngles);
         
            didISpawn = 0;
        }
        int seperateCounterChud = 1;
        int alternate = 1;
        
        for (int i = seperateCounterChud - didISpawn; i < counter; i++)
        {
            Debug.Log("I spawned even more");
            var meteorInfo = (attackInfo as MeteorShower);
            var mainAttack = meteorInfo.meteorInfo.WeaponEffect(owner, NetworkManager.Singleton.LocalTime.TimeAsFloat, owner.transform.position, owner.transform.eulerAngles, 1, owner.transform.position, owner.transform.eulerAngles);
            mainAttack.transform.position += seperateCounterChud * alternate * Vector3.right * attackGap;
            mainAttack.transform.eulerAngles += new Vector3(0, 0, 180 / counter * seperateCounterChud * alternate);
            alternate *= -1;
      
            if(alternate == 1)
            {
                seperateCounterChud += 1;
            }

            if (meteorInfo.meteorSound)
                mainAttack.GetComponent<AttackSoundPlayer>().PlaySoundRpc(NetworkData.Instance.audioDataBase.GetId[meteorInfo.meteorSound]);
           
        }
  
        Destroy(gameObject);
        
        
    }
    public override void OnTriggerEnter(Collider other)
    {

       
    }
}
