using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class Crouching : MonoBehaviour
{
    public enum CrouchState
    {
        Crouched,
        Standing
    }

    public enum SlamAction
    {
        Explode,
        Implode,
        None
    }
    
    [SerializeField] float crouchDownForce;
    public SlamAction slamAction;
    [HideInInspector] public bool canSlam;

    [ShowIf(nameof(IsSlamActionExplodeOrImplode))] public float actionRadius;
    [ShowIf(nameof(IsSlamActionExplodeOrImplode))] public float actionForce;

    [ShowIf(nameof(IsSlamActionExplode))] public float explosionUpForce;

    CrouchState crouchState = CrouchState.Standing;
    GroundCheck groundCheckScript;
    AnimationController animController;
    Rigidbody rig;
    
    void Awake()
    {
        animController = GetComponent<AnimationController>();
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Get GroundCheck Script Reference
        groundCheckScript = GetComponent<GroundCheck>();
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                StartCrouch();
                break;
            case InputActionPhase.Canceled:
                StopCrouch();
                break;
        } 
    }

    void FixedUpdate()
    {
        if (crouchState == CrouchState.Crouched)
        {
            WhileCrouch();
        }
    }

    void StartCrouch() // Called when crouch input is pressed
    {
        if (groundCheckScript.groundState == GroundCheck.GroundState.Airborne)
            canSlam = true;

        crouchState = CrouchState.Crouched;

        Debug.Log("Start Crouch");

        animController.Crouch();
    }

    void WhileCrouch() // Called while crouch input is pressed [FixedUpdate]
    {
        // Apply downforce
        if (canSlam)
        {
            rig.AddForce(-transform.up * crouchDownForce, ForceMode.Acceleration);
        }

        Debug.Log("While Crouch");
    }

    void StopCrouch() // Called when crouch input is released
    {
        canSlam = false;

        crouchState = CrouchState.Standing;
        
        Debug.Log("Stop Crouch");

        animController.UnCrouch();
    }

    // Helper methods for NaughtyAttributes
    private bool IsSlamActionExplodeOrImplode()
    {
        return slamAction == SlamAction.Explode || slamAction == SlamAction.Implode;
    }

    private bool IsSlamActionExplode()
    {
        return slamAction == SlamAction.Explode;
    }
}
