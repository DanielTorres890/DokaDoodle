using UnityEngine;

public class PlayQuickSFX : MonoBehaviour
{
    public AudioClip sfx;

    public void PlaySound()
    {
        SFXManager.Instance.PlaySFX(sfx);
    }
}
