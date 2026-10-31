using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Trap Item Object", menuName = "Inventory System/Items/TargetDebuff")]
public class TargetDebuffItem : ItemBase
{
    public BuffBase targetDebuff;

    public override void ItemInfoCheck(int player, int itemId)
    {
        if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }


        ClientChecks.Instance.ShowConfirmItemButtonsRpc(player, itemId, 0);

        UnityAction subscription = null;
        subscription = () =>
        {
            Subscribe(player, itemId);

        };

        ClientChecks.Instance.onItemUse.AddListener(subscription);
        ClientChecks.Instance.onItemUse.AddListener(delegate { ClientChecks.Instance.onItemUse.RemoveListener(subscription); });

    }

    public override void PerformItemEffect(int player, InventoryObject inventory)
    {


        FreeMover.Instance.EndFreeCamera(TileArrowRules.AllEnemies);
        base.PerformItemEffect(player, inventory);
        Debug.Log("we got here so thats pretty cool"); //the reason why we don't deploy the trap here is bc we don't actually wanna use the trap unless the event gets invoked
    }

    private void Subscribe(int player, int itemId)
    {
        FreeMover.Instance.FreeCamera(TileArrowRules.AllEnemies);
        FreeMover.Instance.onTileSelect.AddListener(ValidOption);
       
        FreeMover.Instance.onUndoFree.AddListener(delegate { ClientChecks.Instance.UndoItemUseRpc(); });
        FreeMover.Instance.onUndoFree.AddListener(delegate { ClientChecks.Instance.onItemUse.RemoveListener(delegate { Subscribe(player, itemId); }); });

    }
    public void ValidOption(int selectTile)
    {
        var selectedTile = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][selectTile];
   
        List<EntityStats> allTargets = new List<EntityStats>();
        foreach(var enemy in selectedTile.tileEnemy)
        {
            if(!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
            {
                allTargets.Add(enemy);
            }
        }
        foreach (var player in selectedTile.players)
        {
            var enemy = NetworkData.Instance.players[player];
            if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
            {
                allTargets.Add(enemy);
            }
        }
        foreach (var enemy in selectedTile.partyMembers)
        {
            if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
            {
                allTargets.Add(enemy);
            }
        }

        if(allTargets.Count <= 0) { return; }

        Debug.Log("Am i actually happening here? ");
        foreach(var target in allTargets)
        {
            if(target is EnemyCombat)
            {
                for(int i = 0; i < PlayerMoveManager.Instance.orderedEnemies.Count; i++)
                {
                    if(PlayerMoveManager.Instance.orderedEnemies[i] == target)
                    {
                        ClientChecks.Instance.BuffEnemyRpc(i, NetworkData.Instance.buffDataBase.GetId[targetDebuff]);
                    }
                }

            }
            if(target is playerData)
            {
                playerData player = (playerData)target;
                NetworkData.Instance.AddStatusToPlayerRpc(player.playerNumber, NetworkData.Instance.buffDataBase.GetId[targetDebuff]);
            }
            if(target is PartyMember)
            {
                PartyMember ally = (PartyMember)target;
                NetworkData.Instance.AddStatusToAllyRpc(ally.allyOwner, ally.allyNumber, NetworkData.Instance.buffDataBase.GetId[targetDebuff]);
            }
        }

        if(NetworkManager.Singleton.IsHost)
        ClientChecks.Instance.ConfirmBuffRpc(NetworkData.Instance.GetCurrentPlayer().playerNumber, NetworkData.Instance.playerInventories[0][0].database.GetId[this], 0);
    }
}
