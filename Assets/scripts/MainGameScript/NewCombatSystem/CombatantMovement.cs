using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatantMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody body;

    [SerializeField] private GameObject playerCam;
    [SerializeField] private float speed, sensitivy, maxForce;
    private Vector2 move, look;
    private float lookRotation;

    public void moveForward(InputAction.CallbackContext action)
    {
        move = action.action.ReadValue<Vector2>();

    }
    public void LookAround(InputAction.CallbackContext action)
    {
        look = action.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 currentVelocity = body.velocity;
        Vector3 targetVeloctiy = new Vector3(move.x,0,move.y);
        targetVeloctiy *= speed;

        targetVeloctiy = transform.TransformDirection(targetVeloctiy);

        Vector3 velocityChange = (targetVeloctiy - currentVelocity);

        Vector3.ClampMagnitude(velocityChange, maxForce);

        body.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    void Update()
    {
        
    }
}
