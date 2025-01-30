using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [HideInInspector] public MoveState moveState;
    [HideInInspector] public MoveDirection moveDirection;
    [HideInInspector] public GroundState groundState;
    [HideInInspector] public WallState wallState;
    [HideInInspector] public InputType inputType;

    public Rigidbody rig;

    [HideInInspector] public Vector2 moveInputValue;

    [HideInInspector] public float lastGroundedTime;

    public Transform cameraContainer;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();

        moveState = MoveState.Idle;
        moveDirection = MoveDirection.None;
        groundState = GroundState.Airborne;
        wallState = WallState.None;
    }
}
