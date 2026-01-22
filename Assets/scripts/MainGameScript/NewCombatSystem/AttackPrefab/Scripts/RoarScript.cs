using System.Collections;
using UnityEngine;

public class RoarScript : MonoBehaviour
{
    
    void Start()
    {
        //I gotta be honest im not a huge fan but this is much faster tha doing some bs
        StartCoroutine(DelayParticles());
    }

    private IEnumerator DelayParticles()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        var emit = new ParticleSystem.EmitParams();

        emit.velocity = gameObject.transform.TransformDirection(Vector3.back * 5f);
        emit.rotation = 90;
        ps.Emit(emit, 1);

        emit.velocity = gameObject.transform.TransformDirection(Vector3.forward * 5f);
        emit.rotation = 270f;
        ps.Emit(emit, 1);

        yield return new WaitForSeconds(0.3f);
        emit.velocity = gameObject.transform.TransformDirection(Vector3.back * 5f);
        emit.rotation = 90;
        ps.Emit(emit, 1);

        emit.velocity = gameObject.transform.TransformDirection(Vector3.forward * 5f);
        emit.rotation = 270f;
        ps.Emit(emit, 1);

        yield return new WaitForSeconds(0.3f);
        emit.velocity = gameObject.transform.TransformDirection(Vector3.back * 5f);
        emit.rotation = 90;
        ps.Emit(emit, 1);

        emit.velocity = gameObject.transform.TransformDirection(Vector3.forward * 5f);
        emit.rotation = 270f;
        ps.Emit(emit, 1);
    }
}
