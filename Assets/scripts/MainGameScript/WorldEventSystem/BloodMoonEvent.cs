using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/BloodMoon")]
public class BloodMoonEvent : TimedWEvent
{

    public float monsterMultiplier = 1;
    public float monsterGoldMultiplier = 1;
    public float monsterXpMultiplier = 1;

    public override void OnActivate(int randomNum)
    {
        base.OnActivate(randomNum);
        NetworkData.Instance.globalEnemyMultiplier *= monsterMultiplier;
        NetworkData.Instance.globalEnemyMoneyDrop *= monsterGoldMultiplier;
        NetworkData.Instance.globalEnemyXpDrop *= monsterXpMultiplier;
    }
    public override void OnDeactivate()
    {

        base.OnDeactivate();
        NetworkData.Instance.globalEnemyMultiplier /= monsterMultiplier;
        NetworkData.Instance.globalEnemyMoneyDrop /= monsterGoldMultiplier;
        NetworkData.Instance.globalEnemyXpDrop /= monsterXpMultiplier;
    }
}
