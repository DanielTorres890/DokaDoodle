using UnityEngine;

public class FPSManager : MonoBehaviour
{
    [SerializeField]
    private int targetFPS = 60; // Set your desired target FPS here

    void Awake()
    {
        // Disable VSync in the Editor to allow custom target framerate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }
}