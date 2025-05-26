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
            commonVariables.SetMoveInput(context.ReadValue<Vector2>());
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
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
        CrouchState crouchState = commonVariables.GetCrouchState();
        WallState wallState = commonVariables.GetWallState();
        MoveDirection moveDirection = commonVariables.GetMoveDirection();

        switch (groundState)
        {
            case GroundState.Grounded:
                if (crouchState == CrouchState.Crouched)
                {
                    rig.AddRelativeForce(new Vector3(commonVariables.GetMoveInput().x, 0, commonVariables.GetMoveInput().y) * crouchMoveSpeed, ForceMode.Acceleration);
                }
                else if (crouchState == CrouchState.Sliding)
                {
                    rig.AddRelativeForce(new Vector3(commonVariables.GetMoveInput().x, 0, commonVariables.GetMoveInput().y) * slideMoveSpeed, ForceMode.Acceleration);
                }
                else
                {
                    rig.AddRelativeForce(new Vector3(commonVariables.GetMoveInput().x, 0, commonVariables.GetMoveInput().y) * onGroundMoveSpeed, ForceMode.Acceleration);
                }
                break;
            case GroundState.WallGrounded:
                Vector2 correctedInput = commonVariables.GetMoveInput();
                if (wallState == WallState.Right)
                {
                    correctedInput = new Vector2(Mathf.Clamp(commonVariables.GetMoveInput().x, -1, 0), commonVariables.GetMoveInput().y);
                }
                else if (wallState == WallState.Left)
                {
                    correctedInput = new Vector2(Mathf.Clamp(commonVariables.GetMoveInput().x, 0, 1), commonVariables.GetMoveInput().y);
                }

                if (correctedInput.x == 0 && commonVariables.GetMoveInput().x != 0)
                {
                    if (moveDirection == MoveDirection.Forward || moveDirection == MoveDirection.ForwardRight || moveDirection == MoveDirection.ForwardLeft)
                        correctedInput.y = Mathf.Sqrt(Mathf.Clamp01(1 - (commonVariables.GetMoveInput().x * commonVariables.GetMoveInput().x))) * 1.4f;
                    else if (moveDirection == MoveDirection.Back || moveDirection == MoveDirection.BackRight || moveDirection == MoveDirection.BackLeft)
                        correctedInput.y = -Mathf.Sqrt(Mathf.Clamp01(1 - (commonVariables.GetMoveInput().x * commonVariables.GetMoveInput().x))) * 1.4f;
                }
                rig.AddRelativeForce(new Vector3(correctedInput.x, 0, correctedInput.y) * onWallMoveSpeed, ForceMode.Acceleration);
                break;
            case GroundState.Airborne:
                rig.AddRelativeForce(new Vector3(commonVariables.GetMoveInput().x, 0, commonVariables.GetMoveInput().y) * inAirMoveSpeed, ForceMode.Acceleration);
                break;
        }
        // Determine the direction of movement
        if (commonVariables.GetMoveInput().y > MOVE_THRESHOLD)
        {
            if (commonVariables.GetMoveInput().x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardRight;
            }
            else if (commonVariables.GetMoveInput().x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardLeft;
            }
            else
            {
                moveDirection = MoveDirection.Forward;
            }
        }
        else if (commonVariables.GetMoveInput().y < -MOVE_THRESHOLD)
        {
            if (commonVariables.GetMoveInput().x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackRight;
            }
            else if (commonVariables.GetMoveInput().x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackLeft;
            }
            else
            {
                moveDirection = MoveDirection.Back;
            }
        }
        else
        {
            if (commonVariables.GetMoveInput().x > MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Right;
            }
            else if (commonVariables.GetMoveInput().x < -MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.Left;
            }
        }

        commonVariables.SetMoveDirection(moveDirection);
    }

    void StopMoving()
    {
        onStopWalk.Invoke();
        commonVariables.SetMoveDirection(MoveDirection.None);
    }

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
