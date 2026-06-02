using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "New Enemy", menuName = "Combat System/EnemyEncounter")]
public class EnemyEncounter : ScriptableObject
{
    public EnemyBase[] enemies;
    public bool isRaid = false;
    public string EncounterName;
    public AudioClip battleMusic;

}
