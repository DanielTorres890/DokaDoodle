using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/NoEnemy")]
public class TileClearCondition : QuestCondition
{
    public EnemyBase enemyCheck;
    public int mapId;
    public int tileId;

    public bool shouldContain;
    public override bool CanBeginQuest()
    {
        if (MapTileSpecialEvents.Instance.mapTiles.Length <= mapId) { return shouldContain; }

        if (MapTileSpecialEvents.Instance.mapTiles[mapId] == null) { return shouldContain; }

        if (MapTileSpecialEvents.Instance.mapTiles[mapId][tileId] == null) { return shouldContain; }

        foreach (var enemy in MapTileSpecialEvents.Instance.mapTiles[mapId][tileId].tileEnemy)
        {
            if (PlayerCombatManager.Instance.EnemyDataBase.GetId[enemyCheck] == enemy.enemyId) {  return shouldContain; }
        }

        return !shouldContain;

    }
}
