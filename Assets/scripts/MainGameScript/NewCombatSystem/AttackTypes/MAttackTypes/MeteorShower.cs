using UnityEngine;


[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/MagicAttacks/Meteor Shower")]
public class MeteorShower : AttackBase
{

    public int meteorCount;
    public GameObject meteor;
    [Tooltip("The time between each meteor spawn")]
    public float interval;

    [Tooltip("how far of a starting offset they should have from the player")]
    public Vector3 meteorOffset;

    [Tooltip("It selects a random direction to fall (will always fall) \nand these values are the maximums itll be offset (instead of falling straight down")]
    public Vector3 directionWidth;

    [Tooltip("This is the sound the meteor itself will make when falling")]
    public AudioClip meteorSound;
}
