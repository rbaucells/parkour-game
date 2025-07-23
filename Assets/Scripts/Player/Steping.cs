using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using System.Linq;

public class Steping : MonoBehaviour
{
    [SerializeField] float maxStepHeight;
    [SerializeField] float maxFloorAngle;

    [Header("Step Durations")]
    [SerializeField][Range(0, 1)] float stepUpDuration;
    [SerializeField][Range(0, 1)] float crouchStepUpDuration;
    [SerializeField][Range(0, 1)] float slideStepUpDuration;

    [Header("References")]
    [SerializeField] Transform stepDetectionCube;
    [SerializeField] Transform StepDetectionCubeParent;
    [SerializeField] LayerMask layerMask;

    BoxCollider stepDetectionCubeCol;
    CommonVariables commonVariables;
    CapsuleCollider col;
    Rigidbody rig;

    const float MOVE_THRESHOLD = 0.1f;
    const float SECONDARY_MOVE_THRESHOLD = 0.4f; // The one for if we moving forward, are we moving forward right/left/middle
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

            if (angleBetweenFloorAndUp <= maxFloorAngle)
            {
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
    }

    void StandingStepUp()
    {  
        float curColCenter = col.center.y;
        float curColHeight = col.height;
        float feetYLevel = transform.position.y + curColCenter - (curColHeight / 2);

        if (feetYLevel < 0.1f)
        {
            feetYLevel = 0f;
        }

        Vector3[] directions = DirectionsToCheckForLedge(feetYLevel);
        float speed = new Vector3(rig.velocity.x, 0, rig.velocity.z).magnitude;
        float feetRayLenght = (0.07f * speed) + 0.51f;

        foreach (Vector3 direction in directions)
        {
            Ray feetRay = new Ray(new Vector3(transform.position.x, feetYLevel + FEET_TOLERANCE, transform.position.z), direction);
            RaycastHit feetHit;

            Debug.DrawRay(feetRay.origin, feetRay.direction * feetRayLenght, Color.red, 5);

            if (Physics.Raycast(feetRay, out feetHit, feetRayLenght, layerMask)) // There is something in the way
            {
                Ray heightCheckRay = new Ray(new Vector3(transform.position.x, feetYLevel + maxStepHeight, transform.position.z), direction);

                Debug.DrawRay(heightCheckRay.origin, heightCheckRay.direction * (feetHit.distance + 0.5f), Color.blue, 5);
                if (!Physics.Raycast(heightCheckRay, feetHit.distance + 0.5f, layerMask)) // it is short enough to walk up
                {
                    Vector3 origin = new Vector3(feetHit.point.x, maxStepHeight + feetYLevel, feetHit.point.z);
                    Ray posFinderRay = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    Debug.DrawRay(posFinderRay.origin, posFinderRay.direction, Color.green, 5);
                    if (Physics.Raycast(posFinderRay, out hit, 1, layerMask))
                    {
                        Vector3 targetPoint = new Vector3(hit.point.x, hit.point.y + (transform.position.y - feetYLevel) + FEET_TOLERANCE, hit.point.z);
                        Debug.Log("targetPoint: " + targetPoint);

                        transform.DOMove(targetPoint, stepUpDuration).OnStart(() =>
                        {
                            commonVariables.SetStepingUp(true);
                        }).OnComplete(() =>
                        {
                            transform.position = new Vector3(targetPoint.x, targetPoint.y, targetPoint.z);
                            commonVariables.SetStepingUp(false);
                        });
                        break;
                    }
                }
            }
        }
    }

    void CrouchedStepUp()
    {
        float curColCenter = col.center.y;
        float curColHeight = col.height;
        float feetYLevel = transform.position.y + curColCenter - (curColHeight / 2);

        if (feetYLevel < 0.1f)
        {
            feetYLevel = 0f;
        }

        Vector3[] directions = DirectionsToCheckForLedge(feetYLevel);
        float speed = new Vector2(rig.velocity.x, rig.velocity.z).magnitude;
        float feetRayLenght = 0.75f;

        foreach (Vector3 direction in directions)
        {
            Ray feetRay = new Ray(new Vector3(transform.position.x, feetYLevel + FEET_TOLERANCE, transform.position.z), direction);
            RaycastHit feetHit;
            Debug.DrawRay(feetRay.origin, feetRay.direction * feetRayLenght, Color.red, 5);

            if (Physics.Raycast(feetRay, out feetHit, feetRayLenght, layerMask)) // There is something in the way
            {
                Ray heightCheckRay = new Ray(new Vector3(transform.position.x, feetYLevel + maxStepHeight, transform.position.z), direction);

                Debug.DrawRay(heightCheckRay.origin, heightCheckRay.direction * (feetHit.distance + 0.5f), Color.blue, 5);
                if (!Physics.Raycast(heightCheckRay, feetHit.distance + 0.5f, layerMask)) // it is short enough to walk up
                {
                    Vector3 origin = new Vector3(feetHit.point.x, maxStepHeight + feetYLevel, feetHit.point.z);
                    Ray posFinderRay = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    Debug.DrawRay(posFinderRay.origin, posFinderRay.direction, Color.green, 5);
                    if (Physics.Raycast(posFinderRay, out hit, 1, layerMask))
                    {
                        Vector3 targetPoint = new Vector3(hit.point.x, hit.point.y + (transform.position.y - feetYLevel) + FEET_TOLERANCE, hit.point.z);
                        Debug.Log("targetPoint: " + targetPoint);

                        transform.DOMove(targetPoint, crouchStepUpDuration).OnStart(() =>
                        {
                            commonVariables.SetStepingUp(true);
                        }).OnComplete(() =>
                        {
                            transform.position = new Vector3(targetPoint.x, targetPoint.y, targetPoint.z);
                            commonVariables.SetStepingUp(false);
                        });
                    }
                }
            }
        }
    }

    void SlideStepUp()
    {
        float curColCenter = col.center.y;
        float curColHeight = col.height;
        float feetYLevel = transform.position.y + curColCenter - (curColHeight / 2);

        if (feetYLevel < 0.1f)
        {
            feetYLevel = 0f;
        }

        Vector3[] directions = SlidingDirectionsForLedge(feetYLevel);
        float speed = new Vector3(rig.velocity.x, 0, rig.velocity.z).magnitude;
        float feetRayLenght = (0.2f * speed) + 0.7f;

        foreach (Vector3 direction in directions)
        {
            Ray feetRay = new Ray(new Vector3(transform.position.x, feetYLevel + FEET_TOLERANCE, transform.position.z), direction);
            RaycastHit feetHit;

            if (Physics.Raycast(feetRay, out feetHit, feetRayLenght, layerMask)) // There is something in the way
            {
                Debug.DrawRay(feetRay.origin, feetRay.direction * feetRayLenght, Color.red, 5);
                Ray heightCheckRay = new Ray(new Vector3(transform.position.x, feetYLevel + maxStepHeight, transform.position.z), direction);

                if (!Physics.Raycast(heightCheckRay, feetHit.distance + 0.5f, layerMask)) // it is short enough to walk up
                {
                    Debug.DrawRay(heightCheckRay.origin, heightCheckRay.direction * (feetHit.distance + 0.5f), Color.blue, 5);
                    Vector3 origin = new Vector3(feetHit.point.x, maxStepHeight + feetYLevel, feetHit.point.z);
                    Ray posFinderRay = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    Debug.DrawRay(posFinderRay.origin, posFinderRay.direction, Color.green, 5);
                    if (Physics.Raycast(posFinderRay, out hit, 1, layerMask))
                    {
                        Vector3 targetPoint = new Vector3(hit.point.x, hit.point.y + (transform.position.y - feetYLevel) + FEET_TOLERANCE, hit.point.z);
                        Debug.Log("targetPoint: " + targetPoint);

                        transform.DOMove(targetPoint, slideStepUpDuration).OnStart(() =>
                        {
                            commonVariables.SetStepingUp(true);
                        }).OnComplete(() =>
                        {
                            transform.position = new Vector3(targetPoint.x, targetPoint.y, targetPoint.z);
                            commonVariables.SetStepingUp(false);
                        });
                    }
                }
            }
        }
    }

    Vector3[] SlidingDirectionsForLedge(float feetYLevel)
    {
        MoveDirection moveDirection;

        Vector3 slidingDirection = commonVariables.GetSlidingDirection();
        if (slidingDirection.z > MOVE_THRESHOLD)
        {
            if (slidingDirection.x > SECONDARY_MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardRight;
            }
            else if (slidingDirection.x < -SECONDARY_MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.ForwardLeft;
            }
            else
            {
                moveDirection = MoveDirection.Forward;
            }
        }
        else if (slidingDirection.z < -MOVE_THRESHOLD)
        {
            if (slidingDirection.x > SECONDARY_MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackRight;
            }
            else if (slidingDirection.x < -SECONDARY_MOVE_THRESHOLD)
            {
                moveDirection = MoveDirection.BackLeft;
            }
            else
            {
                moveDirection = MoveDirection.Back;
            }
        }
        else if (slidingDirection.x > MOVE_THRESHOLD)
        {
            moveDirection = MoveDirection.Right;
        }
        else if (slidingDirection.x < -MOVE_THRESHOLD)
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
        Vector3 halfExtent = stepDetectionCubeCol.bounds.extents / 2;
        Vector3 direction = StepDetectionCubeParent.forward;
        Quaternion orientation = transform.rotation;

        // RaycastHit[] raycastHits = Physics.BoxCastAll(center, halfExtent, direction, orientation, Mathf.Infinity, layerMask);

        // if (raycastHits == null)
        //     return new Vector3[0];

        // foreach (RaycastHit raycastHit in raycastHits)
        // {
        //     Vector3 point = new Vector3(raycastHit.point.x, feetYLevel, raycastHit.point.z);
        //     Vector3 ourFeetPosition = new Vector3(transform.position.x, feetYLevel, transform.position.z);
        //     Vector3 directionToPoint = (point - ourFeetPosition).normalized;
        //     directions.Add(directionToPoint);
        // }

        Collider[] colliders = Physics.OverlapBox(center, halfExtent, orientation, layerMask, QueryTriggerInteraction.UseGlobal);

        if (colliders.Count() < 1)
        {
            return new Vector3[0];
        }

        foreach (Collider collider in colliders)
        {
            Debug.Log("collided with collider " + collider.name);
            Vector3 point = collider.ClosestPointOnBounds(new Vector3(transform.position.x, feetYLevel, transform.position.z));
            Vector3 ourFeetPosition = new Vector3(transform.position.x, feetYLevel, transform.position.z);
            Vector3 directionToPoint = (point - ourFeetPosition).normalized;
            directions.Add(directionToPoint);
        }

        return directions.ToArray();
    }

    Vector3[] DirectionsToCheckForLedge(float feetYLevel)
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
        Vector3 halfExtent = stepDetectionCube.localScale / 2;
        // Vector3 direction = StepDetectionCubeParent.forward;
        Quaternion orientation = transform.rotation;

        // RaycastHit[] raycastHits = Physics.BoxCastAll(center, halfExtent, direction, orientation, Mathf.Infinity, layerMask);

        // if (raycastHits.Count() < 1)
        //     return new Vector3[0];

        // foreach (RaycastHit raycastHit in raycastHits)
        // {
        //     Vector3 point = new Vector3(raycastHit.point.x, feetYLevel, raycastHit.point.z);
        //     Vector3 ourFeetPosition = new Vector3(transform.position.x, feetYLevel, transform.position.z);
        //     Vector3 directionToPoint = (point - ourFeetPosition).normalized;
        //     directions.Add(directionToPoint);
        // }

        Collider[] colliders = Physics.OverlapBox(center, halfExtent, orientation, layerMask, QueryTriggerInteraction.UseGlobal);

        if (colliders.Count() < 1)
        {
            return new Vector3[0];
        }

        foreach (Collider collider in colliders)
        {
            Debug.Log("collided with collider " + collider.name);
            Vector3 point = collider.ClosestPointOnBounds(new Vector3(transform.position.x, feetYLevel, transform.position.z));
            Vector3 ourFeetPosition = new Vector3(transform.position.x, feetYLevel, transform.position.z);
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
