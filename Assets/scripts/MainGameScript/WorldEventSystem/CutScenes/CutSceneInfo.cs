using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cutscene", menuName = "WorldEvents/CutsceneInfo")]
public class CutSceneInfo : ScriptableObject
{
    public List<string> dialogue;
    public GameObject cutsceneBackground;


}
