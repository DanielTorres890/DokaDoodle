using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponColorUpdate : NetworkBehaviour
{
    public VisualEffect effect;
    public AbilityBase ability;
   
    public void WeaponColor()
    {

        if (IsHost && ability.ownerStats is playerData && effect && ability.attackInfo.colorByWeapon)
        {

            playerData ownerPlayer = (ability.ownerStats as playerData);
            if (ownerPlayer.equipItems[ItemType.Equipment] == -1) { return; }
           
            WeaponItem weapon = NetworkData.Instance.playerInventories[0][3].database.GetItem[ownerPlayer.equipItems[ItemType.Equipment]] as WeaponItem;
            if (weapon.weaponColor == Color.black) { return; }
          

         
            SyncColorRpc(ownerPlayer.equipItems[ItemType.Equipment]);


        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SyncColorRpc(int equippedItem)
    {
        WeaponItem weapon = NetworkData.Instance.playerInventories[0][3].database.GetItem[equippedItem] as WeaponItem;
        if(effect.HasVector4("Color")) { Debug.Log("I did find it"); }
        effect.SetVector4("Color", weapon.weaponColor);
        effect.Reinit();
        
    }
}
