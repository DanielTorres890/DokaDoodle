using System.Collections;
using UnityEngine;

public class RoarScript : MonoBehaviour
{
    public float angleOffset;
    void Start()
    {
        //I gotta be honest im not a huge fan but this is much faster tha doing some bs
        StartCoroutine(DelayParticles());
    }

    private IEnumerator DelayParticles()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        var emit = new ParticleSystem.EmitParams();

      


        emit.velocity = Vector3.left * 5f;
        emit.rotation = gameObject.transform.eulerAngles.y ;
        emit.startLifetime = .2f;
        ps.Emit(emit, 1);

        emit.velocity = Vector3.right * 5f;
        emit.rotation = gameObject.transform.eulerAngles.y + 180;
        emit.startLifetime = .2f;
        ps.Emit(emit, 1);

        yield return new WaitForSeconds(0.3f);
        emit.velocity = Vector3.left * 5f;
        emit.rotation = gameObject.transform.eulerAngles.y;
        emit.startLifetime = .2f;
        ps.Emit(emit, 1);

        emit.velocity = Vector3.right * 5f;
        emit.rotation = gameObject.transform.eulerAngles.y + 180;
        emit.startLifetime = .2f;
        ps.Emit(emit, 1);

        yield return new WaitForSeconds(0.3f);
        emit.velocity = Vector3.left * 5f;
        emit.rotation = gameObject.transform.eulerAngles.y;
        emit.startLifetime = .2f;
        ps.Emit(emit, 1);

        emit.velocity = Vector3.right * 5f;
        emit.rotation = gameObject.transform.eulerAngles.y + 180;
        emit.startLifetime = .2f;
        ps.Emit(emit, 1);

        yield return new WaitForSeconds(0.3f);
    }
}
