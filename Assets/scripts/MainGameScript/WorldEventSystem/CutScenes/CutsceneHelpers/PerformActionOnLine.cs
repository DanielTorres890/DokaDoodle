using UnityEngine;
using UnityEngine.Events;

public class PerformActionOnLine : MonoBehaviour
{
    public CutsceneManager myManager;

    [Tooltip("Which line number to perform some action")]
    public int lineNumber;

    [Tooltip("Whatever event you want to happen (probably an animation)")]
    public UnityEvent action;

    private void Awake()
    {
        myManager = FindObjectsByType<CutsceneManager>(FindObjectsSortMode.None)[0];
        myManager.dialogueBox.onLineFinish.AddListener(PerformCutscene);
    }

    private void PerformCutscene(int currentLineNumber)
    {
        if(currentLineNumber != lineNumber) { return; }
        action.Invoke();
    }
}
