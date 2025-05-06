using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    AnimationController animController;
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
    [SerializeField] [Range(0, 100)] float gravityForce;

    [SerializeField] [MinMaxSlider(75, 105)] Vector2 wallAngleRange = new Vector2(85, 95);

    [SerializeField] float groundSlamForce;
    
    [HideInInspector] public GroundState groundState { get; private set; } = GroundState.Airborne;
    [HideInInspector] public WallState wallState { get; private set; } = WallState.None;

    [HideInInspector] public float lastGroundedTime;

    Crouching crouchingScript;
    Jumping jumpingScript;
    Movement movementScript;

    ISet<Collider> groundColliders = new HashSet<Collider>();
    ISet<Collider> wallColliders = new HashSet<Collider>();

    const float GROUND_TOLERANCE = 0.3f;

    [Range(0,1)] public float wallRunGravityMultipliers;

    CapsuleCollider col;
    Rigidbody rig;

    void Awake()
    {
        animController = GetComponent<AnimationController>();
        // Get Collider Reference
        col = GetComponent<CapsuleCollider>();
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Get Crouching Script Reference
        crouchingScript = GetComponent<Crouching>();
        // Get Jumping Script Reference
        jumpingScript = GetComponent<Jumping>();
        movementScript = GetComponent<Movement>();
    }

    void FixedUpdate()
    {
        if (groundState != GroundState.WallGrounded)
        {
            // Apply Gravity
            rig.AddForce(-transform.up * gravityForce, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        // Store contact points
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);

        float curColCenter = col.center.y;
        float curColHeight = col.height;

        var feetYLevel = (transform.position.y + curColCenter) - curColHeight / 2;
        // Loop Through them
        foreach (var contactPoint in contactPoints)
        {
            // Check if the contact point is near the feet
            if (Mathf.Abs(contactPoint.point.y - feetYLevel) < GROUND_TOLERANCE)
            {
                // That collider is ground
                groundColliders.Add(other.collider);
                break;
            }
        }

        bool curGrounded = groundColliders.Any();

        // If we are grounded and we were not grounded before
        if (curGrounded && groundState == GroundState.Airborne)
        {
            AirborneToGrounded(other);
        }
        // If we are grounded but we were WallGrounded. Transition
        else if (curGrounded && groundState == GroundState.WallGrounded)
        {
            WallGroundedToGrounded(other);
        }


        if (groundState != GroundState.Grounded)
        {
            // Loop through the contact points
            foreach (var contactPoint in contactPoints)
            {
                Vector3 normal = contactPoint.normal;
                float wallToGroundAngle = Vector3.Angle(normal, Vector3.up);
                
                if (wallToGroundAngle >= wallAngleRange.x && wallToGroundAngle <= wallAngleRange.y)
                {
                    // Get the dot product between the contact point normal and the Transforms directions
                    float dotForward = Vector3.Dot(normal, transform.forward);
                    float dotRight = Vector3.Dot(normal, transform.right);
                    float dotBack = Vector3.Dot(normal, -transform.forward);
                    float dotLeft = Vector3.Dot(normal, -transform.right);

                    if (dotForward > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Back;
                    }
                    else if (dotRight > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Left;
                    }
                    else if (dotBack > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Front;
                    }
                    else if (dotLeft > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Right;
                    }
                    else
                    {
                        wallState = WallState.None;
                    }
                }
            }

            // If we are wall grounded and we were not wall grounded before
            if (groundState == GroundState.Airborne && wallColliders.Any())
            {
                AirborneToWallGrounded();
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (groundState == GroundState.Airborne)
        {
            var contactPoints = new ContactPoint[other.contactCount];
            other.GetContacts(contactPoints);
            // Loop through the contact points
            foreach (var contactPoint in contactPoints)
            {
                Vector3 normal = contactPoint.normal;
                float wallToGroundAngle = Vector3.Angle(normal, Vector3.up);
                
                if (wallToGroundAngle >= wallAngleRange.x && wallToGroundAngle <= wallAngleRange.y)
                {
                    // Get the dot product between the contact point normal and the Transforms directions
                    float dotForward = Vector3.Dot(normal, transform.forward);
                    float dotRight = Vector3.Dot(normal, transform.right);
                    float dotBack = Vector3.Dot(normal, -transform.forward);
                    float dotLeft = Vector3.Dot(normal, -transform.right);

                    if (dotForward > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Back;
                    }
                    else if (dotRight > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Left;
                    }
                    else if (dotBack > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Front;
                    }
                    else if (dotLeft > 0.5f)
                    {
                        wallColliders.Add(other.collider);

                        wallState = WallState.Right;
                    }
                    else
                    {
                        wallState = WallState.None;
                    }
                }
            }
            // If we are wall grounded and we were not wall grounded before
            if (groundState == GroundState.Airborne && wallColliders.Any())
            {
                AirborneToWallGrounded();
            }
        }
        else if (groundState == GroundState.WallGrounded)
        {
            // Apply Gravity
            rig.AddForce(-transform.up * gravityForce * wallRunGravityMultipliers, ForceMode.Acceleration);   
        }
    }

    void OnCollisionExit(Collision other)
    {
        // Remove the collider from the grounded colliders
        groundColliders.Remove(other.collider);

        bool curGrounded = groundColliders.Any();

        // If we are not grounded and we were grounded before
        if (!curGrounded && groundState == GroundState.Grounded)
        {
            GroundedToAirborne();
        }

        // Remove the collider from the wall colliders
        wallColliders.Remove(other.collider);

        bool curWallGrounded = wallColliders.Any();

        // If we are not wall grounded and we were wall grounded before
        if (!curWallGrounded && groundState == GroundState.WallGrounded)
        {
            WallGroundedToAirborne();
        }
    }

    void AirborneToGrounded(Collision other)
    {
        groundState = GroundState.Grounded;

        if (crouchingScript.canSlam)
        {
            // Apply ground slam force
            rig.AddForce(other.GetContact(0).normal * groundSlamForce, ForceMode.Impulse);

            crouchingScript.canSlam = false;

            switch (crouchingScript.slamAction)
            {
                case Crouching.SlamAction.Explode:
                    Boom.Explode(other.GetContact(0).point, crouchingScript.actionRadius, crouchingScript.actionForce, crouchingScript.explosionUpForce);
                    break;
                case Crouching.SlamAction.Implode:
                    Boom.Implode(other.GetContact(0).point, crouchingScript.actionRadius, crouchingScript.actionForce);
                    break;
            }

            switch(movementScript.moveDirection)
            {
                case Movement.MoveDirection.Forward:
                    animController.GroundSlamMiddle(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.Back:
                    animController.GroundSlamMiddle(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.Right:
                    animController.GroundSlamRight(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.ForwardRight:
                    animController.GroundSlamRight(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.BackRight:
                    animController.GroundSlamRight(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.Left:
                    animController.GroundSlamLeft(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.ForwardLeft:
                    animController.GroundSlamLeft(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.BackLeft:
                    animController.GroundSlamLeft(other.relativeVelocity.y);
                    break;
                case Movement.MoveDirection.None:
                    animController.GroundSlamMiddle(other.relativeVelocity.y);
                    break;
            }
        }
        else
        {
            animController.Land(other.relativeVelocity.y);
        }

        if (jumpingScript.jumpHeld)
        {
            jumpingScript.GroundedJump();
        }

        jumpingScript.remainingAirJumps = jumpingScript.maxAirJumps;
        jumpingScript.usedCayoteTime = false;
    }

    void GroundedToAirborne()
    {
        groundState = GroundState.Airborne;

        lastGroundedTime = Time.time;
    }

    void WallGroundedToGrounded(Collision other)
    {
        wallColliders.Remove(other.collider);
        WallGroundedToAirborne();

        AirborneToGrounded(other);
    }

    void AirborneToWallGrounded()
    {
        groundState = GroundState.WallGrounded;

        animController.WallRunIn(wallState);

        jumpingScript.remainingAirJumps = jumpingScript.maxAirJumps;
        jumpingScript.usedCayoteTime = false;
    }

    void WallGroundedToAirborne()
    {
        groundState = GroundState.Airborne;

        animController.WallRunOut();

        wallState = WallState.None;

        lastGroundedTime = Time.time;
    }

    void WhileWallGrounded()
    {

    }
}