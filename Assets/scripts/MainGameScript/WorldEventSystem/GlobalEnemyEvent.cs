using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/GlobalEnemyEvent")]
public class GlobalEnemyEvent : TimedWEvent
{
    public EncounterWrapper encounter;
    public override void OnActivate(int randomNum)
    {
        WorldEventManager.Instance.globalEncounterTable.Add(new EncounterAndWeight(encounter));
        base.OnActivate(randomNum);
    }
    public override void OnDeactivate()
    {
        for(int i = 0; i < WorldEventManager.Instance.globalEncounterTable.Count; i++)
        {
            if(WorldEventManager.Instance.globalEncounterTable[i].encounterId == PlayerCombatManager.Instance.EnemyEncounterDataBase.GetId[encounter.encounter])
            {
                WorldEventManager.Instance.globalEncounterTable.RemoveAt(i);
                break;
            }
        }
        base.OnDeactivate();
    }
}
