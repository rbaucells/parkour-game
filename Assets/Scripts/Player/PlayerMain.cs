using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMain : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        Moving
    }

    public enum MoveDirection
    {
        Forward,
        ForwardRight,
        Right,
        BackRight,
        Back,
        BackLeft,
        Left,
        ForwardLeft,
        None
    }

    public enum GroundState
    {
        Grounded,
        WallGrounded,
        Airborne
    }

    public enum WallState
    {
        Right,
        Left,
        Front,
        Back,
        None
    }

    public enum InputType
    {
        Keyboard,
        Controller
    }

    public enum CrouchState
    {
        Crouched,
        Standing
    }

    // Player State Enums
    [HideInInspector] public MoveState moveState = MoveState.Idle;
    [HideInInspector] public MoveDirection moveDirection = MoveDirection.None;
    [HideInInspector] public GroundState groundState = GroundState.Airborne;
    [HideInInspector] public WallState wallState = WallState.None;
    [HideInInspector] public InputType inputType = InputType.Keyboard;
    [HideInInspector] public CrouchState crouchState = CrouchState.Standing;

    // Fields for Multiple Scripts
    [HideInInspector] public bool canSlam;
    [HideInInspector] public float lastGroundedTime;
    // Commomn References
    public Transform cameraContainer;

    void Awake()
    {
        if (Gamepad.all.Count > 0)
            inputType = InputType.Controller;
        else
            inputType = InputType.Keyboard;
    }
}
