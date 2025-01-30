using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundCheck : PlayerMain
{
    CapsuleCollider col;
    
    const float feetTolerance = 0.3f;

    private ISet<Collider> groundColiders = new HashSet<Collider>();
    private ISet<Collider> wallColliders = new HashSet<Collider>();

    public Vector2 wallAngleMinMax = new Vector2(85, 95);

    void Awake()
    {
        // Get Collider Reference
        col = gameObject.GetComponent<CapsuleCollider>();;
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
            if (Mathf.Abs(contactPoint.point.y - feetYLevel) < feetTolerance)
            {
                // That collider is ground
                groundColiders.Add(other.collider);
                break;
            }
        }

        bool curGrounded = groundColiders.Any();

        // If we are grounded and we were not grounded before
        if (curGrounded && groundState != GroundState.Grounded)
        {
            StartGrounded();
        }
        // Loop through the contact points
        foreach (var contactPoint in contactPoints)
        {
            Vector3 normal = contactPoint.normal;
            float wallToGroundAngle = Vector3.Angle(normal, Vector3.up);
            
            if (wallToGroundAngle >= wallAngleMinMax.x && wallToGroundAngle <= wallAngleMinMax.y)
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
        groundColiders.Remove(other.collider);

        bool curGrounded = groundColiders.Any();

        // If we are not grounded and we were grounded before
        if (!curGrounded && groundState == GroundState.Grounded)
        {
            StopGrounded();
        }

        // Remove the collider from the wall colliders
        wallColliders.Remove(other.collider);

        bool curWallGrounded = wallColliders.Any();

        // If we are not wall grounded and we were wall grounded before
        if (curWallGrounded && groundState == GroundState.WallGrounded)
        {
            StopWallGrounded();
        }
    }

    void StartGrounded()
    {
        groundState = GroundState.Grounded;
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
    }

    void StopWallGrounded()
    {
        groundState = GroundState.Airborne;

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

        wallState = WallState.None;

        lastGroundedTime = Time.time;
    }
}
