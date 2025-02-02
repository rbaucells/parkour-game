using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
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

    [SerializeField] int wallAngleMin = 85;
    [SerializeField] int wallAngleMax = 95;

    [SerializeField] float groundSlamForce;
    
    [HideInInspector] public GroundState groundState { get; private set; } = GroundState.Airborne;
    [HideInInspector] public WallState wallState { get; private set; } = WallState.None;

    [HideInInspector] public float lastGroundedTime;

    Crouching crouchingScript;
    Jumping jumpingScript;

    ISet<Collider> groundColliders = new HashSet<Collider>();
    ISet<Collider> wallColliders = new HashSet<Collider>();

    const float GROUND_TOLERANCE = 0.3f;

    CapsuleCollider col;
    Rigidbody rig;

    void Awake()
    {
        // Get Collider Reference
        col = GetComponent<CapsuleCollider>();
        // Get Rigidbody Reference
        rig = GetComponent<Rigidbody>();
        // Get Crouching Script Reference
        crouchingScript = GetComponent<Crouching>();
        // Get Jumping Script Reference
        jumpingScript = GetComponent<Jumping>();
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
        if (curGrounded && groundState != GroundState.Grounded)
        {
            StartGrounded(other);
        }
        // Loop through the contact points
        foreach (var contactPoint in contactPoints)
        {
            Vector3 normal = contactPoint.normal;
            float wallToGroundAngle = Vector3.Angle(normal, Vector3.up);
            
            if (wallToGroundAngle >= wallAngleMin && wallToGroundAngle <= wallAngleMax)
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
            StartWallGrounded();
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
            StopGrounded();
        }

        // Remove the collider from the wall colliders
        wallColliders.Remove(other.collider);

        bool curWallGrounded = wallColliders.Any();

        // If we are not wall grounded and we were wall grounded before
        if (!curWallGrounded && groundState == GroundState.WallGrounded)
        {
            StopWallGrounded();
        }
    }

    void StartGrounded(Collision other)
    {
        groundState = GroundState.Grounded;

        if (crouchingScript.canSlam)
        {
            // Apply ground slam force
            rig.AddForce(other.GetContact(0).normal * groundSlamForce, ForceMode.Impulse);

            Debug.Log("Ground Slam");

            crouchingScript.canSlam = false;
        }

        if (jumpingScript.jumpHeld)
        {
            jumpingScript.GroundedJump();
        }

        jumpingScript.remainingAirJumps = jumpingScript.maxAirJumps;
    }

    void StopGrounded()
    {
        groundState = GroundState.Airborne;

        lastGroundedTime = Time.time;
    }

    void StartWallGrounded()
    {
        groundState = GroundState.WallGrounded;

        switch (wallState)
        {
            case WallState.Right:
                Debug.Log("Enter Right");
                break;
            case WallState.Left:
                Debug.Log("Enter Left");
                break;
            case WallState.Front:
                Debug.Log("Enter Front");
                break;
            case WallState.Back:
                Debug.Log("Enter Back");
                break;
        }

        jumpingScript.remainingAirJumps = jumpingScript.maxAirJumps;
    }

    void StopWallGrounded()
    {
        groundState = GroundState.Airborne;

        switch (wallState)
        {
            case WallState.Right:
                Debug.Log("Exit Right");
                break;
            case WallState.Left:
                Debug.Log("Exit Left");
                break;
            case WallState.Front:
                Debug.Log("Exit Front");
                break;
            case WallState.Back:
                Debug.Log("Exit Back");
                break;
        }

        wallState = WallState.None;

        lastGroundedTime = Time.time;
    }
}
