using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class GroundCheck : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] [Range(0, 200)] float gravityForce;
    [SerializeField] [Range(0,1)] float wallRunGravityMultipliers;

    [Header("Wall Running")]
    [SerializeField] [MinMaxSlider(75, 105)] Vector2 wallAngleRange = new Vector2(85, 95);
    ISet<Collider> wallColliders = new HashSet<Collider>();
    [Header("Events")]
    public UnityEvent<Collision> onAirborneToGrounded = new UnityEvent<Collision>();
    public UnityEvent<WallState> onAirborneToWallGrounded = new UnityEvent<WallState>();
    public UnityEvent onGroundedToAirborne = new UnityEvent();
    public UnityEvent onWallGroundedToAirborne = new UnityEvent();
    public UnityEvent onWallGroundedToGrounded = new UnityEvent();
    public UnityEvent<WallState> onWallStateChange = new UnityEvent<WallState>();

    // ground detection
    ISet<Collider> groundColliders = new HashSet<Collider>();
    const float GROUND_TOLERANCE = 0.3f;
    const float MAX_FLOOR_ANGLE = 45f;
    // component references
    CapsuleCollider col;
    Rigidbody rig;
    CommonVariables commonVariables;

    void Awake()
    {
        // get component references
        col = GetComponent<CapsuleCollider>();
        rig = GetComponent<Rigidbody>();
        commonVariables = GetComponent<CommonVariables>();
    }

    void FixedUpdate()
    {
        bool stepping = commonVariables.GetStepingUp();

        if (!stepping)
        {
            if (commonVariables.GetGroundState() != GroundState.WallGrounded)
            {
                rig.AddForce(-transform.up * gravityForce, ForceMode.Acceleration);
            }
            else
            {
                rig.AddForce(-transform.up * gravityForce * wallRunGravityMultipliers, ForceMode.Acceleration);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);

        float curColCenter = col.center.y;
        float curColHeight = col.height;

        float feetYLevel = (transform.position.y + curColCenter) - curColHeight / 2;
        foreach (var contactPoint in contactPoints)
        {
            if (Mathf.Abs(contactPoint.point.y - feetYLevel) < GROUND_TOLERANCE)
            {
                groundColliders.Add(other.collider);
                break;
            }
        }

        bool curGrounded = groundColliders.Any();
        GroundState groundState = commonVariables.GetGroundState();

        if (curGrounded && groundState == GroundState.Airborne)
        {
            AirborneToGrounded(other);
        }
        else if (curGrounded && groundState == GroundState.WallGrounded)
        {
            WallGroundedToGrounded(other);
        }


        if (groundState != GroundState.Grounded)
        {
            foreach (var contactPoint in contactPoints)
            {
                Vector3 normal = contactPoint.normal;
                float wallToGroundAngle = Vector3.Angle(normal, Vector3.up);

                if (wallToGroundAngle >= wallAngleRange.x && wallToGroundAngle <= wallAngleRange.y)
                {
                    float dotForward = Vector3.Dot(normal, transform.forward);
                    float dotRight = Vector3.Dot(normal, transform.right);
                    float dotBack = Vector3.Dot(normal, -transform.forward);
                    float dotLeft = Vector3.Dot(normal, -transform.right);

                    if (dotForward > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Back);
                    }
                    else if (dotRight > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Left);
                    }
                    else if (dotBack > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Front);
                    }
                    else if (dotLeft > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Right);
                    }
                    else
                    {
                        commonVariables.SetWallState(WallState.None);
                    }
                }
            }

            if (groundState == GroundState.Airborne && wallColliders.Any())
            {
                AirborneToWallGrounded(other);
            }
        }

        if (groundState == GroundState.Grounded)
        {
            foreach (ContactPoint contactPoint in contactPoints)
            {
                if (Vector3.Angle(contactPoint.normal, Vector3.up) < MAX_FLOOR_ANGLE)
                {
                    commonVariables.SetGroundNormal(contactPoint.normal);
                    break;
                }
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);

        GroundState groundState = commonVariables.GetGroundState();

        if (groundState != GroundState.Grounded)
        {
            wallColliders.Clear();
            foreach (var contactPoint in contactPoints)
            {
                Vector3 normal = contactPoint.normal;
                float wallToGroundAngle = Vector3.Angle(normal, Vector3.up);

                if (wallToGroundAngle >= wallAngleRange.x && wallToGroundAngle <= wallAngleRange.y)
                {
                    float dotForward = Vector3.Dot(normal, transform.forward);
                    float dotRight = Vector3.Dot(normal, transform.right);
                    float dotBack = Vector3.Dot(normal, -transform.forward);
                    float dotLeft = Vector3.Dot(normal, -transform.right);

                    WallState oldWallState = commonVariables.GetWallState();
                    if (dotForward > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Back);
                    }
                    else if (dotRight > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Left);
                    }
                    else if (dotBack > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Front);
                    }
                    else if (dotLeft > 0.5f)
                    {
                        wallColliders.Add(other.collider);
                        commonVariables.SetWallState(WallState.Right);
                    }
                    else
                    {
                        commonVariables.SetWallState(WallState.None);
                    }

                    if (oldWallState != commonVariables.GetWallState())
                        onWallStateChange.Invoke(commonVariables.GetWallState());
                }
                else
                {
                    wallColliders.Remove(other.collider);
                    WallGroundedToAirborne();
                }
            }
            if (groundState == GroundState.Airborne && wallColliders.Any())
            {
                AirborneToWallGrounded(other);
            }
        }

        if (groundState == GroundState.Grounded)
        {
            foreach (ContactPoint contactPoint in contactPoints)
            {
                if (Vector3.Angle(contactPoint.normal, Vector3.up) < MAX_FLOOR_ANGLE)
                {
                    commonVariables.SetGroundNormal(contactPoint.normal);
                    break;
                }
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        GroundState groundState = commonVariables.GetGroundState();

        groundColliders.Remove(other.collider);

        bool curGrounded = groundColliders.Any();

        if (!curGrounded && groundState == GroundState.Grounded)
        {
            GroundedToAirborne();
        }

        wallColliders.Remove(other.collider);

        bool curWallGrounded = wallColliders.Any();

        if (!curWallGrounded && groundState == GroundState.WallGrounded)
        {
            WallGroundedToAirborne();
        }
    }

    void AirborneToGrounded(Collision other)
    {
        commonVariables.SetGroundState(GroundState.Grounded);
        onAirborneToGrounded.Invoke(other);
        commonVariables.SetGroundNormal(other.GetContact(0).normal);
    }

    void GroundedToAirborne()
    {
        commonVariables.SetGroundState(GroundState.Airborne);
        onGroundedToAirborne.Invoke();
        commonVariables.SetGroundNormal(Vector3.zero);
    }

    void WallGroundedToGrounded(Collision other)
    {
        commonVariables.SetGroundState(GroundState.Grounded);
        onWallGroundedToGrounded.Invoke();
        commonVariables.SetGroundNormal(other.GetContact(0).normal);
    }

    void AirborneToWallGrounded(Collision other)
    {
        commonVariables.SetGroundState(GroundState.WallGrounded);
        onAirborneToWallGrounded.Invoke(commonVariables.GetWallState());
        commonVariables.SetWallNormal(other.GetContact(0).normal);
        rig.velocity = new(rig.velocity.x, 0, rig.velocity.z);
    }

    void WallGroundedToAirborne()
    {
        commonVariables.SetWallNormal(Vector3.zero);
        commonVariables.SetGroundState(GroundState.Airborne);
        commonVariables.SetWallState(WallState.None);
        onWallGroundedToAirborne.Invoke();
    }
}   