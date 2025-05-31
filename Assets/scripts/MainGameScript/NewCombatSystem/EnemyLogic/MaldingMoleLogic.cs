using Unity.VisualScripting;
using UnityEngine;

public class MaldingMoleLogic : BaseEnemyBehavior
{

    public float[] attackRanges;

    public override bool InRange()
    {
        var distance = Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position);
        
        for (int i = 0; i < myManager.orderedAttacks.Count; i++)
        {
            if (distance < attackRanges[i] && myManager.stateManager[myManager.orderedAttacks[i]].cooldown <= 0) { return true; }
        }
        return false;
    }
    public override void selectAttack()
    {
        var distance = Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position);

        for (int i = 0; i < myManager.orderedAttacks.Count; i++)
        {
            if (distance < attackRanges[i] && myManager.stateManager[myManager.orderedAttacks[i]].cooldown <= 0) 
            {
                selectedAttack = myManager.orderedAttacks[i];
                return;
            }
        }
        Debug.Log("apparently i just didn't select one unlucky");
    }
}
