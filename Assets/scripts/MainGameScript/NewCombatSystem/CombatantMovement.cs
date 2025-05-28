using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatantMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody body;

    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private GameObject playerCam;
    [SerializeField] private float speed, sensitivy, maxForce, jumpForce;


    [SerializeField] private PlayerInput action;


    private Vector2 move, look;
    private float lookRotation;

    private float dashCdTimer;
    [SerializeField] private float dashCd;

    //private Camera camcomponent;

    //All of my states.. hopefully?
    [SerializeField] private bool grounded;

    public void moveForward(InputAction.CallbackContext action)
    {

        move = action.action.ReadValue<Vector2>();

    }
    
    public void LookAround(InputAction.CallbackContext action)
    {
        look = action.action.ReadValue<Vector2>();
    }
    public void NormalJump(InputAction.CallbackContext action)
    {

        if (grounded && IsOwner) 
        {
      
            body.AddForce(Vector3.up * (jumpForce + abilityManager.stats.speedFormula()), ForceMode.VelocityChange);
        }
    }
    public void DashRight(InputAction.CallbackContext action)
    {
        if(dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }
        if (grounded && IsOwner)
        {
            dashCdTimer = 0;
            abilityManager.combatantstate = combatantStates.Dashing;
            abilityManager.stateDuration = .25f;
            body.AddForce(transform.TransformDirection(Vector3.right * (jumpForce + abilityManager.stats.dashFormula())), ForceMode.Impulse);
        }
    }
    public void DashLeft(InputAction.CallbackContext action)
    {
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }
        if (grounded && IsOwner)
        {
            dashCdTimer = 0;
            abilityManager.combatantstate = combatantStates.Dashing;
            abilityManager.stateDuration = .25f;
            body.AddForce(transform.TransformDirection(Vector3.left * (jumpForce + abilityManager.stats.dashFormula())), ForceMode.Impulse);
        }
    }

    public void DashFwd(InputAction.CallbackContext action)
    {
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }
        if (grounded && IsOwner)
        {
            dashCdTimer = 0;
            abilityManager.combatantstate = combatantStates.Dashing;
            abilityManager.stateDuration = .25f;
            body.AddForce(transform.TransformDirection(Vector3.forward * (jumpForce + abilityManager.stats.dashFormula())), ForceMode.Impulse);
        }
    }
    public void DashBack(InputAction.CallbackContext action)
    {
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.combatantstate == combatantStates.Dashing) { return; }
        if (grounded && IsOwner)
        {
            dashCdTimer = 0;
            abilityManager.combatantstate = combatantStates.Dashing;
            abilityManager.stateDuration = .25f;
            body.AddForce(transform.TransformDirection(Vector3.back * (jumpForce + abilityManager.stats.dashFormula())), ForceMode.Impulse);
        }
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) { return; }

        action.actions["DashRight"].performed += DashRight;
        action.actions["DashLeft"].performed += DashLeft;
        action.actions["DashFwd"].performed += DashFwd;
        action.actions["DashBack"].performed += DashBack;
        //camcomponent = playerCam.GetComponent<Camera>();
    }
    private void FixedUpdate()
    {
        if (NewCombatManager.instance.fightOver || !IsOwner) { return; }
  
        Move();   
    }


    private void Move()
    {
        dashCdTimer += Time.deltaTime;
        if (!abilityManager.CanMove()) 
        {
          
            transform.Rotate(new Vector3(playerCam.transform.parent.localEulerAngles.x,0,0));
            playerCam.transform.parent.transform.localRotation = Quaternion.identity;
            body.constraints = RigidbodyConstraints.FreezePosition;
            return;
        }    
        body.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 currentVelocity = body.linearVelocity;

        Vector3 targetVeloctiy;
        targetVeloctiy = new Vector3(move.x, 0, move.y);
        targetVeloctiy *= speed + abilityManager.stats.speedFormula();
        targetVeloctiy = transform.TransformDirection(targetVeloctiy);

        if (abilityManager.CanMoveNotAct())
        {
            
            targetVeloctiy = new Vector3(currentVelocity.x - targetVeloctiy.x, 0, currentVelocity.z - targetVeloctiy.z);
        }


        Vector3 velocityChange = targetVeloctiy - currentVelocity;
        velocityChange = new Vector3(velocityChange.x,0,velocityChange.z);

        Vector3.ClampMagnitude(velocityChange, maxForce);
       
        body.AddForce(velocityChange, ForceMode.VelocityChange);
        if (abilityManager.combatantstate != combatantStates.Attacking && body.angularVelocity.y > maxForce) 
        {
            body.angularVelocity = new Vector3(body.angularVelocity.x, maxForce, body.angularVelocity.z);  
        }


        playerCam.transform.parent.transform.Rotate(new Vector3(transform.eulerAngles.x, 0, 0),Space.Self);
        //camcomponent.transparencySortAxis = new Vector3(math.sin(math.radians(playerCam.transform.parent.transform.eulerAngles.y)),0, math.cos( math.radians(playerCam.transform.parent.transform.eulerAngles.x)));
        transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);

        
        
    }

    private void LateUpdate()
    {
        if (NewCombatManager.instance.fightOver || !IsOwner) { return; }
        if (!abilityManager.CanMove() && abilityManager.combatantstate != combatantStates.Endlag)
        {
            transform.Rotate(new Vector3(-look.y * sensitivy, look.x * sensitivy, 0));
            
            return;
        }
        transform.Rotate(new Vector3(0, look.x * sensitivy, 0));
        playerCam.transform.parent.transform.Rotate(new Vector3(-look.y * sensitivy, 0, 0));
        //lookRotation += (-look.y * sensitivy);

        //playerCam.transform.eulerAngles = new Vector3 (lookRotation, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }

    

    
}

