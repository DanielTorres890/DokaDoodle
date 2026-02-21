using UnityEngine;


public abstract class AttackCondition : ScriptableObject
{


    public abstract bool Condition(AbilityManager user);
    
}
