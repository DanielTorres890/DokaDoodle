using UnityEngine;

public class PlayAnimationOnLine : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AnimLineWrapper[] animInfo;
    public Animator myAnimator;
    public void PlayOnLineNum(int linenumber)
    {
        foreach(var anim in animInfo)
        {
            if(anim.lineNum == linenumber)
            {
                myAnimator.SetTrigger(anim.triggerName);
            }
        }
    }
}

[System.Serializable]
public class AnimLineWrapper
{
    public int lineNum;
    public string triggerName;
}
