using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item Tile Reward", menuName = "ItemTile/TakeDamage")]
public class GetHurt : ItemTileRewards
{
    public int damageTaken;

    public override void GiveReward()
    {
        bool isdead = NetworkData.Instance.GetCurrentPlayer().healHp(-damageTaken);
        if (isdead)
        {
            NetworkData.Instance.GetCurrentPlayer().death();
        }
        ClientChecks.Instance.GotOuchie(damageTaken);
    }
}
