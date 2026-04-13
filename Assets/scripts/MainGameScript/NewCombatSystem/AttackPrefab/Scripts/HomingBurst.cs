using System.Linq;
using UnityEngine;

public class HomingBurst : RangedBurstAbility
{
    public Transform target;
    public float detectionRadius = 15f;
    public float trackingSpeed = 3f;
    public LayerMask targets;
   

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(!IsServer) { return; }

        if(target == null)
        {
            var hits = Physics.OverlapSphere(transform.position, detectionRadius, targets);
            if(hits.Length <= 0) { return; }


            foreach (var hit in hits)
            {
                if (hit != owner && hit.TryGetComponent(out AbilityManager entity))
                {
                    if (!entity.stats.loyaltyTags.Intersect(ownerStats.loyaltyTags).Any())
                    {
                        target = hit.transform;
                        break;
                    }
                }
            }



        }

        if(target == null) { return; }
        Vector3 targetDirection = target.position - transform.position + (Vector3.up * 1.5f);


        body.MoveRotation(Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDirection, trackingSpeed * Time.deltaTime, 0.0f)));

        body.linearVelocity = transform.forward * (attackInfo as MDefault).speed;

    }
}
