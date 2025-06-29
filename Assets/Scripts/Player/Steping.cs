using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;

public class Steping : MonoBehaviour
{
    [SerializeField] float maxStepHeight;

    [SerializeField] LayerMask layerMask;
    [SerializeField] float heightCheckRayLenght;

    [SerializeField][Range(0, 1)] float stepUpDuration;
    [SerializeField][Range(0, 1)] float crouchStepUpDuration;
    [SerializeField][Range(0, 1)] float slideStepUpDuration;

    [SerializeField] Transform stepDetectionCube;
    [SerializeField] Transform StepDetectionCubeParent;

    [SerializeField] float stepForwardForce;
    [SerializeField] float crouchForwardForce;

    [SerializeField] float maxFloorAngle;

    BoxCollider stepDetectionCubeCol;
    CommonVariables commonVariables;
    CapsuleCollider col;
    Rigidbody rig;

    const float MOVE_THRESHOLD = 0.1f;
    const float SECONDARY_MOVE_THRESHOLD = 0.4f; // The one for if we moving forward, are we moving forward right/left/middle

    float curColCenter;
    float curColHeight;
    float feetYLevel;
    const float FEET_TOLERANCE = 0.03f;

    void Start()
    {
        col = GetComponent<CapsuleCollider>();
        commonVariables = GetComponent<CommonVariables>();
        stepDetectionCubeCol = stepDetectionCube.GetComponent<BoxCollider>();
        rig = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!commonVariables.GetStepingUp() && commonVariables.GetGroundState() == GroundState.Grounded)
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, layerMask);
            Vector3 groundNormal = hitInfo.normal;
            Debug.DrawRay(transform.position, Vector3.down, Color.yellow, 5);
            float angleBetweenFloorAndUp = Vector3.Angle(groundNormal, Vector3.up);
            if (angleBetweenFloorAndUp >= maxFloorAngle)
            {
                return;
            }

            curColCenter = col.center.y;
            curColHeight = col.height;
            feetYLevel = (transform.position.y + curColCenter) - curColHeight / 2;

            switch (commonVariables.GetCrouchState())
            {
                case CrouchState.Standing:
                    StandingStepUp();
                    break;
                case CrouchState.Crouched:
                    CrouchedStepUp();
                    break;
                case CrouchState.Sliding:
                    SlideStepUp();
                    break;
            }
        }
    }

    void StandingStepUp()
    {  
        if (feetYLevel < 0.1f)
        {
            feetYLevel = 0f;
        }

        Vector3[] directions = DirectionsToCheckForLedge();
        float speed = new Vector2(rig.velocity.x, rig.velocity.z).magnitude;
        float feetRayLenght = (0.06f * speed) + 0.51f;

        foreach (Vector3 direction in directions)
        {
            Ray feetRay = new Ray(new Vector3(transform.position.x, feetYLevel + FEET_TOLERANCE, transform.position.z), direction);
            RaycastHit feetHit;

            if (Physics.Raycast(feetRay, out feetHit, feetRayLenght, layerMask)) // There is something in the way
            {
                Debug.DrawRay(feetRay.origin, feetRay.direction, Color.red, 5);
                Ray heightCheckRay = new Ray(new Vector3(transform.position.x, feetYLevel + maxStepHeight, transform.position.z), direction);

                if (!Physics.Raycast(heightCheckRay, heightCheckRayLenght, layerMask)) // it is short enough to walk up
                {
                    Debug.DrawRay(heightCheckRay.origin, heightCheckRay.direction, Color.blue, 5);
                    Vector3 origin = feetHit.point + direction * 0.1f + (Vector3.up * maxStepHeight);
                    Ray posFinderRay = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    if (Physics.Raycast(posFinderRay, out hit, Mathf.Infinity, layerMask))
                    {
                        Debug.DrawRay(posFinderRay.origin, posFinderRay.direction, Color.green, 5);
                        Vector3 targetPoint = hit.point + Vector3.up;



                        transform.position = targetPoint;
                        // // col.enabled = false;
                        // transform.DOMoveY(targetPoint.y + 0.15f, stepUpDuration).OnStart(() =>
                        // {
                        //     commonVariables.SetStepingUp(true);
                        //     rig.AddForce(direction * stepForwardForce, ForceMode.Impulse);
                        // }
                        // ).OnComplete(() =>
                        // {
                        //     transform.position = new Vector3(transform.position.x, targetPoint.y, transform.position.z);
                        //     commonVariables.SetStepingUp(false);
                        //     // col.enabled = true;
                        // });
                    }
                }
            }
        }
    }

    void CrouchedStepUp()
    {
        if (feetYLevel < 0.1f)
        {
            feetYLevel = 0f;
        }

        Vector3[] directions = DirectionsToCheckForLedge();
        float speed = new Vector2(rig.velocity.x, rig.velocity.z).magnitude;
        float feetRayLenght = 0.75f;

        foreach (Vector3 direction in directions)
        {
            Ray feetRay = new Ray(new Vector3(transform.position.x, feetYLevel + FEET_TOLERANCE, transform.position.z), direction);
            RaycastHit feetHit;

            if (Physics.Raycast(feetRay, out feetHit, feetRayLenght, layerMask)) // There is something in the way
            {
                Debug.DrawRay(feetRay.origin, feetRay.direction, Color.red, 5);
                Ray heightCheckRay = new Ray(new Vector3(transform.position.x, feetYLevel + maxStepHeight, transform.position.z), direction);

                if (!Physics.Raycast(heightCheckRay, heightCheckRayLenght, layerMask)) // it is short enough to walk up
                {
                    Debug.DrawRay(heightCheckRay.origin, heightCheckRay.direction, Color.blue, 5);
                    Vector3 origin = feetHit.point + direction * 0.1f + (Vector3.up * maxStepHeight);
                    Ray posFinderRay = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    if (Physics.Raycast(posFinderRay, out hit, Mathf.Infinity, layerMask))
                    {
                        Debug.DrawRay(posFinderRay.origin, posFinderRay.direction, Color.green, 5);
                        Vector3 targetPoint = hit.point + Vector3.up;

                        // col.enabled = false;
                        transform.position = hit.point;
                        // transform.DOMoveY(targetPoint.y + 0.15f, crouchStepUpDuration).OnStart(() =>
                        // {
                        //     commonVariables.SetStepingUp(true);
                        //     rig.AddForce(direction * crouchForwardForce, ForceMode.Impulse);
                        // }
                        // ).OnComplete(() =>
                        // {
                        //     transform.position = new Vector3(transform.position.x, targetPoint.y, transform.position.z);
                        //     commonVariables.SetStepingUp(false);
                        //     // col.enabled = true;
                        // });
                    }
                }
            }
        }
    }

    void SlideStepUp()
    {
        if (feetYLevel < 0.1f)
        {
            feetYLevel = 0f;
        }

        Vector3[] directions = SlidingDirectionsForLedge();
        float speed = new Vector2(rig.velocity.x, rig.velocity.z).magnitude;
        float feetRayLenght = (0.06f * speed) + 0.51f;

        foreach (Vector3 direction in directions)
        {
            Ray feetRay = new Ray(new Vector3(transform.position.x, feetYLevel + FEET_TOLERANCE, transform.position.z), direction);
            RaycastHit feetHit;

            if (Physics.Raycast(feetRay, out feetHit, feetRayLenght, layerMask)) // There is something in the way
            {
                Debug.DrawRay(feetRay.origin, feetRay.direction, Color.red, 5);
                Ray heightCheckRay = new Ray(new Vector3(transform.position.x, feetYLevel + maxStepHeight, transform.position.z), direction);

                if (!Physics.Raycast(heightCheckRay, heightCheckRayLenght, layerMask)) // it is short enough to walk up
                {
                    Debug.DrawRay(heightCheckRay.origin, heightCheckRay.direction, Color.blue, 5);
                    Vector3 origin = feetHit.point + direction * 0.1f + (Vector3.up * maxStepHeight);
                    Ray posFinderRay = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    if (Physics.Raycast(posFinderRay, out hit, Mathf.Infinity, layerMask))
                    {
                        Debug.DrawRay(posFinderRay.origin, posFinderRay.direction, Color.green, 5);
                        Vector3 targetPoint = hit.point + Vector3.up;

                        // col.enabled = false;
                        transform.DOMoveY(targetPoint.y + 0.15f, slideStepUpDuration).OnStart(() =>
                        {
                            commonVariables.SetStepingUp(true);
                            rig.AddForce(direction * stepForwardForce, ForceMode.Impulse);
                        }
                        ).OnComplete(() =>
                        {
                            transform.position = new Vector3(transform.position.x, targetPoint.y, transform.position.z);
                            commonVariables.SetStepingUp(false);
                            // col.enabled = true;
                        });
                    }
                }
            }
        }
    }

    Vector3[] SlidingDirectionsForLedge()
    {
        MoveDirection moveDirection;

        Vector2 moveInput = commonVariables.GetSlidingDirection();
        if (moveInput.y > MOVE_THRESHOLD)
        {
            if (moveInput.x > SECONDARY_MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardRight;
            }
            else if (moveInput.x < -SECONDARY_MOVE_THRESHOLD)
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
            if (moveInput.x > SECONDARY_MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackRight;
            }
            else if (moveInput.x < -SECONDARY_MOVE_THRESHOLD)
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

        switch (moveDirection)
        {
            case MoveDirection.Forward:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case MoveDirection.ForwardRight:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 45, 0);
                break;
            case MoveDirection.Right:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case MoveDirection.BackRight:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 135, 0);
                break;
            case MoveDirection.Back:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case MoveDirection.BackLeft:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 225, 0);
                break;
            case MoveDirection.Left:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 270, 0);
                break;
            case MoveDirection.ForwardLeft:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 315, 0);
                break;
            case MoveDirection.None:
            default:
                return new Vector3[0];
        }

        List<Vector3> directions = new List<Vector3>();

        Vector3 center = stepDetectionCube.transform.position;
        Vector3 halfExtent = stepDetectionCubeCol.bounds.extents;
        Vector3 direction = StepDetectionCubeParent.forward;
        Quaternion orientation = transform.rotation;

        RaycastHit[] raycastHits = Physics.BoxCastAll(center, halfExtent, direction, orientation, Mathf.Infinity, layerMask);

        if (raycastHits == null)
            return new Vector3[0];

        foreach (RaycastHit raycastHit in raycastHits)
        {
            Vector3 point = new Vector3(raycastHit.point.x, 0, raycastHit.point.z);
            Vector3 ourFeetPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 directionToPoint = (point - ourFeetPosition).normalized;
            directions.Add(directionToPoint);
        }

        return directions.ToArray();
    }

    Vector3[] DirectionsToCheckForLedge()
    {
        MoveDirection moveDirection = commonVariables.GetMoveDirection();

        switch (moveDirection)
        {
            case MoveDirection.Forward:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case MoveDirection.ForwardRight:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 45, 0);
                break;
            case MoveDirection.Right:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case MoveDirection.BackRight:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 135, 0);
                break;
            case MoveDirection.Back:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case MoveDirection.BackLeft:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 225, 0);
                break;
            case MoveDirection.Left:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 270, 0);
                break;
            case MoveDirection.ForwardLeft:
                StepDetectionCubeParent.localEulerAngles = new Vector3(0, 315, 0);
                break;
            case MoveDirection.None:
            default:
                return new Vector3[0];
        }

        List<Vector3> directions = new List<Vector3>();

        Vector3 center = stepDetectionCube.transform.position;
        Vector3 halfExtent = stepDetectionCubeCol.bounds.extents;
        Vector3 direction = StepDetectionCubeParent.forward;
        Quaternion orientation = transform.rotation;

        RaycastHit[] raycastHits = Physics.BoxCastAll(center, halfExtent, direction, orientation, Mathf.Infinity, layerMask);

        if (raycastHits == null)
            return new Vector3[0];

        foreach (RaycastHit raycastHit in raycastHits)
        {
            Vector3 point = new Vector3(raycastHit.point.x, 0, raycastHit.point.z);
            Vector3 ourFeetPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 directionToPoint = (point - ourFeetPosition).normalized;
            directions.Add(directionToPoint);
        }

        return directions.ToArray();
    }
    
    void OnDrawGizmos()
    {
        Gizmos.matrix = stepDetectionCube.transform.localToWorldMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
