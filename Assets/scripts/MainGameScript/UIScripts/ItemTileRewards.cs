using UnityEngine;


public abstract class ItemTileRewards : ScriptableObject
{
    public string rewardName;
    public Sprite rewardSprite;

    public abstract void GiveReward();
}
