using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
[CreateAssetMenu(fileName = "New Default Object", menuName = "PlayerClass/Hunter")]
public class Hunter : PlayerClassBase
{
    public BaseTrap HunterTrap;
    public PlayerClassBase prerequisiteClass;
    public override void ClassAction(playerData player, int randomNum)
    {
        if (!NetworkData.Instance.IsAllowed(player.playerNumber, NetworkManager.Singleton.LocalClientId)) { return; }

        FreeMover.Instance.FreeCamera();
        FreeMover.Instance.onTileSelect.AddListener(HunterTrap.DeployTrap);
        FreeMover.Instance.onTileSelect.AddListener(delegate { base.ClassAction(player, randomNum); });
        FreeMover.Instance.onTileSelect.AddListener(delegate { FreeMover.Instance.EndFreeCamera(); });
    }
    public override bool UnlockCondition(playerData player)
    {
        int hunterId = NetworkData.Instance.classDataBase.GetId[prerequisiteClass];
        if (player.playerClassProgress[hunterId].level > prerequisiteClass.classXpRequirements.Length - 1) { return true; }
        return false;
    }
}
