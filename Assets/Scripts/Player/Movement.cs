using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
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

    [SerializeField] float moveSpeed = 5f;

    [SerializeField] [Range(0, 100)] float gravityForce;

    MoveState moveState = MoveState.Idle;
    [HideInInspector] public MoveDirection moveDirection { get; private set; } = MoveDirection.None;

    const float MOVE_THRESHOLD = 0.1f;

    Vector2 moveInputValue;
    Rigidbody rig;

    void Awake()
    {
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        // Store the input value
        if (context.phase == InputActionPhase.Performed)
        {
            moveInputValue = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            moveInputValue = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        bool curMoving = (moveInputValue.magnitude > MOVE_THRESHOLD);
        // If we are moving
        if (curMoving)
        {
            // If we were idle, start moving
            if (moveState == MoveState.Idle)
            {
                StartMoving();
            }
            // Set move state
            moveState = MoveState.Moving;

            WhileMoving();
        }
        else
        {
            // If we were moving, stop moving
            if (moveState == MoveState.Moving)
            {
                StopMoving();
            }

            moveState = MoveState.Idle;
        }
        // Apply Gravity
        rig.AddForce(-transform.up * gravityForce, ForceMode.Acceleration);
    }

    void StartMoving()
    {
        // Start looping move anim
        Debug.Log("Start Moving");
    }

    void WhileMoving()
    {
        // Move the player
        rig.AddRelativeForce(new Vector3(moveInputValue.x, 0, moveInputValue.y) * moveSpeed, ForceMode.Acceleration);

        // Determine the direction of movement
        if (moveInputValue.y > MOVE_THRESHOLD)
        {
            if (moveInputValue.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardRight;

                Debug.Log("Forward Right");
            }
            else if (moveInputValue.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardLeft;

                Debug.Log("Forward Left");
            }
            else
            {
                moveDirection = MoveDirection.Forward;

                Debug.Log("Forward");
            }
        }
        else if (moveInputValue.y < -MOVE_THRESHOLD)
        {
            if (moveInputValue.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackRight;

                Debug.Log("Back Right");
            }
            else if (moveInputValue.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackLeft;

                Debug.Log("Back Left");
            }
            else
            {
                moveDirection = MoveDirection.Back;

                Debug.Log("Back");
            }
        }
        else
        {
            if (moveInputValue.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Right;

                Debug.Log("Right");
            }
            else if (moveInputValue.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Left;

                Debug.Log("Left");
            }
        }

        Debug.Log("While Moving");
    }

    void StopMoving()
    {
        // Stop looping move anim and transition to idle
        Debug.Log("Stop Moving");

        moveDirection = MoveDirection.None;
    }
}
