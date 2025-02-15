using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{

    [SerializeField] private CombatantMovement combatantController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == combatantController.gameObject) { return; }

        combatantController.SetGrounded(true);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == combatantController.gameObject) { return; }

        combatantController.SetGrounded(false);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == combatantController.gameObject) { return; }

        combatantController.SetGrounded(true);
    }
}
 