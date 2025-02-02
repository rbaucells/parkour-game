using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    [SerializeField] float crouchDownForce;

    [HideInInspector] public bool canSlam;

    CrouchState crouchState = CrouchState.Standing;
    GroundCheck groundCheckScript;
    Rigidbody rig;
    
    void Awake()
    {
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
    }
}
