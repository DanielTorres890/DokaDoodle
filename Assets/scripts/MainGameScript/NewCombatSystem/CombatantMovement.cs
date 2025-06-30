using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatantMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody body;

    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private GameObject playerCam;
    [SerializeField] private float speed, sensitivy, maxForce, jumpForce, minXCam, maxXCam;


    [SerializeField] private PlayerInput action;
    [SerializeField] private characterEditor characterEditor;

    private Vector2 move, look;


    private float dashCdTimer;


    private Vector3 lastDashDirection;
    private float lastDashTime;
    [SerializeField] private float TimeBetweenDash = 0.2f;
    [SerializeField] private float dashCd;

    //private Camera camcomponent;

    //All states are in the ability manager bc honestly it makes more sense there
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
        float oldTime = lastDashTime;
        lastDashTime = Time.fixedTime;
        if (lastDashDirection != Vector3.right)
        {
            lastDashDirection = Vector3.right;
            return;
        }

        if (dashCdTimer < dashCd) { return; }
        if (abilityManager.CanMoveNotAct()) { return; }

        if (!(lastDashTime - oldTime < TimeBetweenDash)) { return; }

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
        float oldTime = lastDashTime;
        lastDashTime = Time.fixedTime;
        if (lastDashDirection != Vector3.left)
        {
            lastDashDirection = Vector3.left;
            return;
        }
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }

        if (!(lastDashTime - oldTime < TimeBetweenDash)) { return; }
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
        float oldTime = lastDashTime;
        lastDashTime = Time.fixedTime;
        if (lastDashDirection != Vector3.forward)
        {
            lastDashDirection = Vector3.forward;
            return;
        }
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }

        if (!(lastDashTime - oldTime < TimeBetweenDash)) { return; }

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
        float oldTime = lastDashTime;
        lastDashTime = Time.fixedTime;
        if (lastDashDirection != Vector3.back)
        {
            lastDashDirection = Vector3.back;
            return;
        }
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }

        if (!(lastDashTime - oldTime < TimeBetweenDash)) { return; }

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
        if (!IsOwner) { return; }

        action.actions["DashRight"].started += DashRight;
        action.actions["DashLeft"].started += DashLeft;
        action.actions["DashFwd"].started += DashFwd;
        action.actions["DashBack"].started += DashBack;

        //big idk from me seems weirde to make seperate
        //camcomponent = playerCam.GetComponent<Camera>();
    }
    private void FixedUpdate()
    {
        
        if (NewCombatManager.instance.fightOver || !IsOwner) 
        {
            body.constraints = RigidbodyConstraints.FreezeAll; 
            return; 
        }

        Move();
    }


    private void Move()
    {
        dashCdTimer += Time.deltaTime;
        if (!abilityManager.CanMove())
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
            transform.Rotate(new Vector3(playerCam.transform.parent.localEulerAngles.x, 0, 0));
            playerCam.transform.parent.transform.localRotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            
            body.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationZ;
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
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        Vector3.ClampMagnitude(velocityChange, maxForce);

        body.AddForce(velocityChange, ForceMode.VelocityChange);
        if (abilityManager.combatantstate != combatantStates.Attacking && body.linearVelocity.y > maxForce) //fmcl
        {
           
            body.linearVelocity = new Vector3(body.linearVelocity.x, maxForce, body.linearVelocity.z);
        }

        //This line makes the camera not snap immediately to back to its previous position but idk how i really wanna handle this yet tbh
        playerCam.transform.parent.transform.Rotate(new Vector3(transform.eulerAngles.x, 0, 0), Space.Self);
        
        
        //camcomponent.transparencySortAxis = new Vector3(math.sin(math.radians(playerCam.transform.parent.transform.eulerAngles.y)),0, math.cos( math.radians(playerCam.transform.parent.transform.eulerAngles.x)));
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);



    }

    private void LateUpdate()
    {
        if (NewCombatManager.instance.fightOver || !IsOwner) { return; }
        if (!abilityManager.CanMove() )
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
            
            transform.Rotate(new Vector3(-look.y * sensitivy, look.x * sensitivy, 0));
            
            if (transform.eulerAngles.x % 360 < 360 + minXCam && transform.eulerAngles.x % 360 > maxXCam)
            {
                transform.Rotate(new Vector3(look.y * sensitivy, 0, 0));
            }
            return;
        }
        transform.Rotate(new Vector3(0, look.x * sensitivy, 0));

       
        playerCam.transform.parent.transform.Rotate(new Vector3(-look.y * sensitivy, 0, 0));

        if (playerCam.transform.parent.transform.eulerAngles.x % 360 < 360 + minXCam && playerCam.transform.parent.transform.eulerAngles.x % 360 > maxXCam)
        {
            playerCam.transform.parent.transform.Rotate(new Vector3(look.y * sensitivy, 0, 0));
        }
        
        
        //lookRotation += (-look.y * sensitivy);

        //playerCam.transform.eulerAngles = new Vector3 (lookRotation, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }


    

}

