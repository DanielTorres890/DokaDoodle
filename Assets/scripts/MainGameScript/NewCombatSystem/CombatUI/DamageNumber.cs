using PrimeTween;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public TextMeshPro dmgText;

    public float minFontSize;
    public float maxFontSize;

    public float lifeTime;
    public float yIncrease;

    public float duration;
    public void SetUp(AbilityManager hitTarget, int damageAmount, bool wasCrit)
    {
        transform.position = hitTarget.transform.position;
        transform.position += new Vector3(UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f));
        dmgText = GetComponent<TextMeshPro>();
        dmgText.text = damageAmount.ToString();
        
        if(wasCrit)
        {
            dmgText.fontSize = minFontSize;
            Sequence.Create()
            .Group(Tween.ShakeLocalPosition(transform, Vector3.right * .5f, 9999))
            .Group(Tween.LocalPositionY(transform, transform.localPosition.y + yIncrease / 2f, 2))
            .Group(Tween.TextFontSize(dmgText, Mathf.Clamp(maxFontSize * damageAmount / hitTarget.stats.stats[Attributes.MaxHealth], minFontSize, maxFontSize) * 1.5f, duration / 2f))
            .Group(Tween.Alpha(dmgText, 0, duration, startDelay: duration / 2f))
            ;
        }
        else
        {
            dmgText.fontSize = Mathf.Clamp(maxFontSize * damageAmount / hitTarget.stats.stats[Attributes.MaxHealth], minFontSize, maxFontSize);
            dmgText.fontSize *= 1.5f;
            Sequence.Create()
            .Group(Tween.Alpha(dmgText, 0, duration))
            .Group(Tween.LocalPositionY(transform, transform.localPosition.y + yIncrease, 2));
        }
        if (NetworkData.Instance.players[NetworkData.Instance.ClientNumToPlayerNum(NetworkManager.Singleton.LocalClientId)] == hitTarget.stats)
        {
            dmgText.color = Color.red;
        }
    }
}
