using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : PlayerMain
{
    const float moveThreshold = 0.1f;

    public float moveSpeed = 5f;

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
        bool curMoving = (Mathf.Abs(moveInputValue.x) >= moveThreshold) && (Mathf.Abs(moveInputValue.y) >= moveThreshold);

        if (curMoving)
        {
            // If we were idle, start moving
            if (moveState == MoveState.Idle)
            {
                StartMoving();
            }

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
        if (moveInputValue.y > moveThreshold)
        {
            if (moveInputValue.x > moveThreshold)
            {
                Debug.Log("Forward Right");
            }
            else if (moveInputValue.x < -moveThreshold)
            {
                Debug.Log("Forward Left");
            }
            else
            {
                Debug.Log("Forward");
            }
        }
        else if (moveInputValue.y < -moveThreshold)
        {
            if (moveInputValue.x > moveThreshold)
            {
                Debug.Log("Back Right");
            }
            else if (moveInputValue.x < -moveThreshold)
            {
                Debug.Log("Back Left");
            }
            else
            {
                Debug.Log("Back");
            }
        }
        else
        {
            if (moveInputValue.x > moveThreshold)
            {
                Debug.Log("Right");
            }
            else if (moveInputValue.x < -moveThreshold)
            {
                Debug.Log("Left");
            }
        }

        Debug.Log("While Moving");
    }

    void StopMoving()
    {
        // Stop looping move anim and transition to idle
        Debug.Log("Stop Moving");
    }
}
