using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetAnim : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
