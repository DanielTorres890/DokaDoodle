using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cutscene", menuName = "WorldEvents/CutsceneInfo")]
public class CutSceneInfo : ScriptableObject
{
    public List<string> dialogue;
    [Tooltip("If you want a delay it should match which dialogue its going to")]
    public List<float> delays;

    public GameObject cutsceneBackground;
    public AudioClip backgroundMusic;

}
