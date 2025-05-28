using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Mathematics;
using UnityEngine.Events;

public class Movement : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        Moving
    }
    MoveState moveState = MoveState.Idle;

    [Header("Move Settings")]
    [SerializeField] float onGroundMoveSpeed;
    [SerializeField] float inAirMoveSpeed;
    [SerializeField] float onWallMoveSpeed;
    [SerializeField] float crouchMoveSpeed;
    [SerializeField] float slideMoveSpeed;
    const float MOVE_THRESHOLD = 0.1f;

    [Header("Drag Settings")]
    [SerializeField] float groundedDrag;
    [SerializeField] float airborneDrag;
    [SerializeField] float wallGroundedDrag;
    [SerializeField] float crouchedDrag;
    [SerializeField] float slideDrag;

    [Header("Unity Events")]
    public UnityEvent<MoveDirection, float> onStartWalk = new UnityEvent<MoveDirection, float>();
    public UnityEvent onStopWalk = new UnityEvent();

    [Header("References")]
    [SerializeField] TextMeshProUGUI speedText;
    Rigidbody rig;
    CommonVariables commonVariables;
    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        commonVariables = GetComponent<CommonVariables>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        // Store the input value
        if (context.phase == InputActionPhase.Performed)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            MoveDirection moveDirection;

            if (moveInput.y > MOVE_THRESHOLD)
            {
                if (moveInput.x > MOVE_THRESHOLD)
                {
                    moveDirection = MoveDirection.ForwardRight;
                }
                else if (moveInput.x < -MOVE_THRESHOLD)
                {
                    moveDirection = MoveDirection.ForwardLeft;
                }
                else
                {
                    moveDirection = MoveDirection.Forward;
                }
            }
            else if (moveInput.y < -MOVE_THRESHOLD)
            {
                if (moveInput.x > MOVE_THRESHOLD)
                {
                    moveDirection = MoveDirection.BackRight;
                }
                else if (moveInput.x < -MOVE_THRESHOLD)
                {
                    moveDirection = MoveDirection.BackLeft;
                }
                else
                {
                    moveDirection = MoveDirection.Back;
                }
            }
            else if (moveInput.x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Right;
            }
            else if (moveInput.x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Left;
            }
            else
            {
                moveDirection = MoveDirection.None;
            }
            commonVariables.SetMoveDirection(moveDirection);
            commonVariables.SetMoveInput(moveInput);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            commonVariables.SetMoveInput(Vector2.zero);

            commonVariables.SetMoveDirection(MoveDirection.None);
            commonVariables.SetMoveInput(Vector2.zero);
        }
    }

    void FixedUpdate()
    {
        bool curMoving = commonVariables.GetMoveInput().magnitude > MOVE_THRESHOLD;
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

        speedText.text = "Speed Text: " + Mathf.RoundToInt(new Vector2(rig.velocity.x, rig.velocity.z).magnitude).ToString();
    }

    void StartMoving()
    {
        onStartWalk.Invoke(commonVariables.GetMoveDirection(), rig.velocity.magnitude);
    }

    void WhileMoving()
    {
        GroundState groundState = commonVariables.GetGroundState();
        switch (groundState)
        {
            case GroundState.Grounded:
                GroundedMove();
                break;
            case GroundState.WallGrounded:
                WallGroundedMove();
                break;
            case GroundState.Airborne:
                AirborneMove();
                break;
        }
    }

    void GroundedMove()
    {
        Vector2 moveInput = commonVariables.GetMoveInput();
        Vector3 moveVector = new(moveInput.x, 0, moveInput.y);
        Vector3 groundNormal = commonVariables.GetGroundNormal();

        Vector3 projectedVector = Vector3.ProjectOnPlane(moveVector, groundNormal);

        Vector3 correctedMagnitudeVector = projectedVector * (moveVector.magnitude / projectedVector.magnitude);

        CrouchState crouchState = commonVariables.GetCrouchState();

        switch (crouchState)
        {
            case CrouchState.Standing:
                rig.AddRelativeForce(correctedMagnitudeVector * onGroundMoveSpeed, ForceMode.Acceleration);
                break;
            case CrouchState.Crouched:
                rig.AddRelativeForce(correctedMagnitudeVector * crouchMoveSpeed, ForceMode.Acceleration);
                break;
            case CrouchState.Sliding:
                rig.AddRelativeForce(correctedMagnitudeVector * slideMoveSpeed, ForceMode.Acceleration);
                break;

        }
    }

    void WallGroundedMove()
    {
        WallState wallState = commonVariables.GetWallState();
        Vector2 moveInput = commonVariables.GetMoveInput();

        if (wallState == WallState.Left || wallState == WallState.Right)
        {
            Vector3 wallNormal = commonVariables.GetWallNormal();
            Vector3 forwardVector = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;

            Vector3 moveVector = ((forwardVector * moveInput.y)).normalized * onWallMoveSpeed;

            switch (wallState)
            {
                case WallState.Right:
                    if (moveInput.x < 0)
                    {
                        rig.AddRelativeForce(moveInput * onWallMoveSpeed, ForceMode.Acceleration);
                    }
                    break;
                case WallState.Left:
                    if (moveInput.x > 0)
                    {
                        rig.AddRelativeForce(moveInput * onWallMoveSpeed, ForceMode.Acceleration);
                    }
                    break;
            }

            rig.AddForce(moveVector, ForceMode.Acceleration);
        }
        else
        {
            switch (wallState)
            {
                case WallState.Back:
                    if (moveInput.y > 0)
                    {
                        rig.AddRelativeForce(new(0, 0, moveInput.y * onWallMoveSpeed), ForceMode.Acceleration);
                    }
                    break;
                case WallState.Front:
                    if (moveInput.y < 0)
                    {
                        rig.AddRelativeForce(new(0, 0, moveInput.y * onWallMoveSpeed), ForceMode.Acceleration);
                    }
                    break;
            }
        }

        // if camera facing too far away from wall, leave it.
    }

    void AirborneMove()
    {
        Vector2 moveInput = commonVariables.GetMoveInput();
        rig.AddRelativeForce(new Vector3(moveInput.x, 0, moveInput.y) * inAirMoveSpeed, ForceMode.Acceleration);
    }

    void StopMoving()
    {
        onStopWalk.Invoke();
        commonVariables.SetMoveDirection(MoveDirection.None);
    }

    // Drag Functions
    public void SetDragGround()
    {
        rig.drag = groundedDrag;
    }
    public void SetDragAir()
    {
        rig.drag = airborneDrag;
    }
    public void SetDragWall()
    {
        rig.drag = wallGroundedDrag;
    }
    public void SetDragCrouch()
    {
        rig.drag = crouchedDrag;
    }
    public void SetDragSlide()
    {
        rig.drag = slideDrag;
    }
    public void SetDragUnCrouch()
    {
        switch (commonVariables.GetGroundState())
        {
            case GroundState.Grounded:
                rig.drag = groundedDrag;
                break;
            case GroundState.WallGrounded:
                rig.drag = wallGroundedDrag;
                break;
            case GroundState.Airborne:
                rig.drag = airborneDrag;
                break;
        }
    }
}
