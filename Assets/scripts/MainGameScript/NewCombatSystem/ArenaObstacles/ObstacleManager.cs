using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ObstacleManager : AbilityManager
{
    public int hp;
    public Rigidbody body;
    private float minFontSize = 5;
    private float maxFontSize = 30;
    private float knockBack = 10f;
    private float explosionRadius = 5;
    public override void OnNetworkSpawn()
    {
        

    }
    public override void Update()
    {

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public override void ImHitRpc(int damageAmt)
    {
        GameObject dmgNum = Instantiate(damageNumber);
        dmgNum.transform.position = transform.position;
        dmgNum.transform.position += new Vector3(UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f));
        var textComponent = dmgNum.GetComponent<TextMeshPro>();
        textComponent.text = damageAmt.ToString();
        textComponent.fontSize = Mathf.Clamp(100f * damageAmt / stats.stats[Attributes.MaxHealth], minFontSize, maxFontSize);
        hp -= damageAmt;
        if(hp < 0 && !stats.isDead)
        {
            body.constraints = RigidbodyConstraints.None;
            stats.isDead = true;
            if(IsHost)
            body.AddExplosionForce(knockBack, transform.position - Vector3.down, explosionRadius, knockBack, ForceMode.Impulse);
        }
    }
}
