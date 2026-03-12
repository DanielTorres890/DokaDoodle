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

    public CombatAnimator animator;
    private Vector2 move, look,scroll;
    public Vector3 additionalForces;

    public float dashCdTimer;


    [SerializeField] private Vector3 lastDashDirection;
    private float lastDashTime;
    [SerializeField] private float TimeBetweenDash = 0.2f;
    [SerializeField] public float dashCd;
    [SerializeField] public int DashDexRequirement;
    [SerializeField] private float DashEnergyCost = 10f;
    //private Camera camcomponent;

    //All states are in the ability manager bc honestly it makes more sense there
    [SerializeField] private bool grounded;
    [SerializeField] private float gravityMult = 2;
    [SerializeField] private float minZoomIn, maxZoomOut;

    public void Start()
    {
        
    }

    public void moveForward(InputAction.CallbackContext action)
    {

        move = action.action.ReadValue<Vector2>();

    }

    public void LookAround(InputAction.CallbackContext action)
    {
        look = action.action.ReadValue<Vector2>();
    }
    public void Scroll(InputAction.CallbackContext action)
    {
        scroll = action.action.ReadValue<Vector2>();
        scroll = scroll.normalized;
    }

    public void NormalJump(InputAction.CallbackContext action)
    {

        if (grounded && IsOwner)
        {
            if(body)
            body.AddForce(Vector3.up * (jumpForce + abilityManager.stats.speedFormula()), ForceMode.VelocityChange);
        }
    }
    //public void DashRight(InputAction.CallbackContext action)
    //{
    //    if (lastDashDirection != Vector3.right)
    //    {
    //        lastDashDirection = Vector3.right;
    //        return;
    //    }
    //    if (grounded && IsOwner)
    //    {
    //        Dash();
    //    }
    //}
    //public void DashLeft(InputAction.CallbackContext action)
    //{
    //    if (lastDashDirection != Vector3.left)
    //    {
    //        lastDashDirection = Vector3.left;
    //        return;
    //    }
      
    //    if (grounded && IsOwner)
    //    {
    //        Dash();
    //    }
    //}

    //public void DashFwd(InputAction.CallbackContext action)
    //{
    //    if (lastDashDirection != Vector3.forward)
    //    {
    //        lastDashDirection = Vector3.forward;
    //        return;
    //    }
       

    //    if (grounded && IsOwner)
    //    {
    //        Dash();
    //    }
    //}
    //public void DashBack(InputAction.CallbackContext action)
    //{
        
    //    if (lastDashDirection != Vector3.back)
    //    {
    //        lastDashDirection = Vector3.back;
    //        return;
    //    }
        

    //    if (grounded && IsOwner)
    //    {
    //        Dash();
    //    }
    //}
    public void Dash(InputAction.CallbackContext action)
    {
        
        if(abilityManager.stats.isDead) {  return; }
        if (dashCdTimer < dashCd) { return; }

        if (abilityManager.CanMoveNotAct()) { return; }

        if (abilityManager.currentEnergy < 10 ) { return; }

        
        if (DashDexRequirement > abilityManager.stats.stats[Attributes.Dexterity]) { return; }

        if(body.linearVelocity.magnitude < .01) { return; }

        abilityManager.currentEnergy -= DashEnergyCost;
        abilityManager.onEnergyChange.Invoke();
        dashCdTimer = 0;
        abilityManager.combatantstate = combatantStates.Dashing;
        abilityManager.stateDuration = .25f;
        if(body)
        {
            // body.AddForce(transform.TransformDirection(new Vector3(move.x, 0, move.y) * (jumpForce + abilityManager.stats.dashFormula())), ForceMode.Impulse);
            body.AddForce(new Vector3(body.linearVelocity.x, 0, body.linearVelocity.z).normalized * abilityManager.stats.dashFormula(), ForceMode.Impulse);
        }

    }
    public override void OnNetworkSpawn()
    {
        
        //big idk from me seems weirde to make seperate
        //camcomponent = playerCam.GetComponent<Camera>();
    }
    private void FixedUpdate()
    {
        
        if (NewCombatManager.instance.fightOver || !IsOwner) 
        {
            
            return; 
        }

        Move();
    }

   
    public void SetUp()
    {

        if (!IsOwner) { return; }

        //action = NewCombatManager.instance.playercontrol;

        //action.actions.actionMaps[0].actions[0].performed += moveForward;
        //action.actions.actionMaps[0].actions[2].performed += LookAround;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 1].performed += Scroll;
        //action.actions.actionMaps[0].actions[3].performed += NormalJump;


        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 5].performed += DashRight;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 4].performed += DashLeft;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 3].performed += DashFwd;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 2].performed += DashBack;

        //action.actions.actionMaps[0].actions[0].started += moveForward;
        //action.actions.actionMaps[0].actions[2].started += LookAround;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 1].started += Scroll;
        //action.actions.actionMaps[0].actions[3].started += NormalJump;


        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 5].started += DashRight;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 4].started += DashLeft;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 3].started += DashFwd;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 2].started += DashBack;

        //action.actions.actionMaps[0].actions[0].canceled += moveForward;
        //action.actions.actionMaps[0].actions[2].canceled += LookAround;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 1].canceled += Scroll;
        //action.actions.actionMaps[0].actions[3].canceled += NormalJump;


        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 5].canceled += DashRight;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 4].canceled += DashLeft;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 3].canceled += DashFwd;
        //action.actions.actionMaps[0].actions[action.actions.actionMaps[0].actions.Count - 2].canceled += DashBack;

    }

    private void Move()
    {
        if (abilityManager.stats.isDead) { return; }

        dashCdTimer += Time.deltaTime;

        
        if (!abilityManager.CanMove())
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
            transform.Rotate(new Vector3(playerCam.transform.parent.localEulerAngles.x, 0, 0));
            playerCam.transform.parent.transform.localRotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            
            body.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationZ;
            animator.WalkingState(false);
            return;
        }
        body.constraints = RigidbodyConstraints.FreezeRotation;
        
        Vector3 currentVelocity = body.linearVelocity;
        animator.WalkingState(true);
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

        //if you're not moving U SHOULDNT MOVE (coould change this later)
        if(move.magnitude > 0) { velocityChange += additionalForces; }
        else { animator.WalkingState(false); }

            body.AddForce(velocityChange, ForceMode.VelocityChange);
        additionalForces = Vector3.zero;
        body.AddForce(Vector3.down * gravityMult, ForceMode.Acceleration);

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
            
            transform.Rotate(new Vector3(-look.y * sensitivy, look.x * sensitivy, 0) * SettingsManager.instance.mouseSense);
            
            if (transform.eulerAngles.x % 360 < 360 + minXCam && transform.eulerAngles.x % 360 > maxXCam)
            {
                transform.Rotate(new Vector3(look.y * sensitivy, 0, 0) * SettingsManager.instance.mouseSense);
            }
            return;
        }
        transform.Rotate(new Vector3(0, look.x * sensitivy, 0) * SettingsManager.instance.mouseSense);

        playerCam.transform.localPosition = new Vector3(playerCam.transform.localPosition.x, playerCam.transform.localPosition.y, Mathf.Clamp(playerCam.transform.localPosition.z + scroll.y, minZoomIn, maxZoomOut));
        playerCam.transform.parent.transform.Rotate(new Vector3(-look.y * sensitivy, 0, 0) * SettingsManager.instance.mouseSense);

        if (playerCam.transform.parent.transform.eulerAngles.x % 360 < 360 + minXCam && playerCam.transform.parent.transform.eulerAngles.x % 360 > maxXCam)
        {
            playerCam.transform.parent.transform.Rotate(new Vector3(look.y * sensitivy, 0, 0) * SettingsManager.instance.mouseSense);
        }
        
        
        //lookRotation += (-look.y * sensitivy);

        //playerCam.transform.eulerAngles = new Vector3 (lookRotation, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }


    

}

