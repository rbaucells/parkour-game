using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public enum CrouchState
{
    Crouched,
    Standing,
    Sliding
}

public enum WallState
{
    Right,
    Left,
    Front,
    Back,
    None
}

public enum GroundState
{
    Grounded,
    WallGrounded,
    Airborne
}

public class CommonVariables : MonoBehaviour
{
    MoveDirection moveDirection = MoveDirection.None;
    GroundState groundState = GroundState.Airborne;
    WallState wallState = WallState.None;
    CrouchState crouchState = CrouchState.Standing;

    Vector2 moveInput;

    public void SetCrouchState(CrouchState state)
    {
        crouchState = state;
    }
    public void SetMoveDirection(MoveDirection direction)
    {
        moveDirection = direction;
    }

    public void SetGroundState(GroundState state)
    {
        groundState = state;
    }

    public void SetWallState(WallState state)
    {
        wallState = state;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public MoveDirection GetMoveDirection()
    {
        return moveDirection;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public GroundState GetGroundState()
    {
        return groundState;
    }

    public WallState GetWallState()
    {
        return wallState;
    }

    public CrouchState GetCrouchState()
    {
        return crouchState;
    }
}
