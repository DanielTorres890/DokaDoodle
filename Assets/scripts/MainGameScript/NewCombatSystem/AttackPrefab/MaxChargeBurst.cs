using UnityEngine;
using UnityEngine.VFX;

public class MaxChargeBurst : MonoBehaviour
{
    public float maxChargeTime;
    public VisualEffect effect;
    private float chargeTimer;

    private bool bursted = false;
    void Start()
    {
        
    }

    void Update()
    {
        if(bursted) { return; }
        chargeTimer += Time.deltaTime;
        if(chargeTimer > maxChargeTime)
        {
            bursted = true;
            effect.Play();
        }
    }
}
