using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class Crouching : MonoBehaviour
{
    [SerializeField] PlayerMain playerMain;
    
    [SerializeField] float crouchDownForce;

    Rigidbody rig;
    
    void Awake()
    {
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
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
        if (playerMain.crouchState == PlayerMain.CrouchState.Crouched)
        {
            WhileCrouch();
        }
    }

    void StartCrouch() // Called when crouch input is pressed
    {
        if (playerMain.groundState == PlayerMain.GroundState.Airborne)
            playerMain.canSlam = true;

        Debug.Log("Start Crouch");
    }

    void WhileCrouch() // Called while crouch input is pressed [FixedUpdate]
    {
        // Apply downforce
        if (playerMain.canSlam)
        {
            rig.AddForce(-transform.up * crouchDownForce, ForceMode.Acceleration);
        }

        Debug.Log("While Crouch");
    }

    void StopCrouch() // Called when crouch input is released
    {
        playerMain.canSlam = false;
        
        Debug.Log("Stop Crouch");
    }
}
