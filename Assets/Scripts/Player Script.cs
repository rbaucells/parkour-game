using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;
using UnityEditor.EditorTools;
using System;

public class PlayerScript : MonoBehaviour
{
    public enum CrouchType
    {
        Hold,
        Toggle
    };

    public enum GroundSlamAction
    {
        Explode,
        Implode
    };

    //--------------------Camera--------------------//
    [Header("Camera")]
    public float controllerLookSens;
    public float mouseLookSens;

    private float lookSenseToUse;

    [Tooltip("Up and Down Rotation")] public float maxX;
    [Tooltip("Up and Down Rotation")] public float minX;

    [HideInInspector] public float curCameraX; // Up and Down

    private Vector2 deltaMouseValue;

    //--------------------Movement--------------------//
    [Header("Movement")]
    public float moveForce;
    private float defaultMoveForce;

    private Vector2 moveValue; // Current Keyboard/Controller input

    private float currentXSpeed;
    private float currentYSpeed;
    private float currentZSpeed;

    private const float movementThreshold = 0.1f;

    public float moveForceIncreasePerSecond;
    public float absoluteMaxSpeed;

    //--------------------Ground Check--------------------//
    [Header("Ground Check")]
    private bool grounded = false;
    private bool wasGrounded;

    private ISet<Collider> groundColiders = new HashSet<Collider>();
    private const float feetTolerance = 0.3f;

    //--------------------Jumping--------------------//
    [Header("Jumping")]

    public float jumpForce;
    public float gravityForce;

    private bool jumpHeld = false;
    public int maxAirJumps;
    private int remainingAirJumps;

    //--------------------Crouching--------------------//
    [Header("Crouching")]
    public CrouchType crouchType;

    private bool crouching;
    public float crouchTransitionTime;

    private float defaultCamHeight;
    private float defaultColHeight;
    private float defaultColCenter;

    [HideInInspector] public float curCamHeight;
    [HideInInspector] public float curColHeight;
    [HideInInspector] public float curColCenter;

    [Space(10)]

    public float crouchTargetCamHeight;
    public float crouchTargetColHeight;
    public float crouchTargetColCenter;

    public AnimationCurve crouchCurve;

    [Space(10)]

    [Tooltip("Add onto down and up force")] public float crouchForcesAddPerSlam;
    public float timeToResetCrouchForces;
    private float defaultCrouchDownForce;
    public float crouchDownForce;
    public float maxCrouchDownForce;

    //--------------------Ground Slam--------------------//
    [Header("Ground Slam")]
    public GroundSlamAction groundSlamAction;

    private float defaultGroundSlamUpForce;
    public float groundSlamUpForce;
    public float maxGroundSlamUpForce;

    private bool allowGroundSlam;
    private float lastGroundSlamTime;

    //--------------------Explode--------------------//
    [Header("Explode")]
    public float groundSlamExplodeRadius;
    public float groundSlamExplodeForce;
    public float groundSlamExplodeUpForce;

    private Vector3[] raycastDirections;
    //--------------------Implode--------------------//
    [Header("Implode")]
    public float groundSlamImplodeRadius;
    public float groundSlamImplodeForce;
    //--------------------Wall Running--------------------//
    [Header("Wall Running")]
    public float wallAttractionForce;
    public float moveMultiplier;
    public float gravityMultiplier;

    public Vector2 wallAngleMinMax;

    [Header("Wall Jumping")]

    private bool wallGrounded = false;
    private bool wasWallGrounded;

    private bool isWallOnRight;
    private bool isWallOnLeft;
    private bool isWallInFront;
    private bool isWallInBack;

    private const float wallTolerance = 0.1f; // Tolerance for horizontal proximity to wall
    private const float wallHeightThreshold = 1.0f; // Height difference to check walls relative to player

    
    private ISet<Collider> wallColliders = new HashSet<Collider>();
    //--------------------Grapple--------------------//
    [Header("Grapple")]
    public float maxGrappleDistance;
    public float force;
    private bool grappling;
    //--------------------References--------------------//
    [Header("References")]
    public TextMeshProUGUI speedText;
    private CapsuleCollider capsuleCollider;
    private Transform cameraContainer;
    private Animator camAnim;
    private Rigidbody rig;
    public Transform shootCenter;

    //--------------------Weapons--------------------//
    [Header("Weapons")]
    private GameObject[] gunScripts;
    private PlayerInput playerInput;
    private InputAction fire;

    //--------------------Effects--------------------//
    [Header("Effects")]
    public GameObject groundSlamParticleSystem;

    void Awake()
    {
        InitializeComponents();
        SetDefaults();
    }

    void InitializeComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        rig = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        gunScripts = GameObject.FindGameObjectsWithTag("Gun");
        cameraContainer = GameObject.Find("Camera Container").transform;
        camAnim = cameraContainer.GetComponent<Animator>();

        fire = playerInput.actions["Shoot"];
    }

    void SetDefaults()
    {
        defaultCamHeight = cameraContainer.transform.localPosition.y;
        defaultColHeight = capsuleCollider.height;
        defaultColCenter = capsuleCollider.center.y;

        curCamHeight = defaultCamHeight;
        curColHeight = defaultColHeight;
        curColCenter = defaultColCenter;

        defaultMoveForce = moveForce;
        defaultCrouchDownForce = crouchDownForce;
        defaultGroundSlamUpForce = groundSlamUpForce;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Every 3 Seconds, Check for input change
        InvokeRepeating(nameof(CheckIfInputChange), 0.0f, 3f);
    }

    void CheckIfInputChange()
    {
        if (Gamepad.all.Count > 0)
            lookSenseToUse = controllerLookSens;
        else
            lookSenseToUse = mouseLookSens;
    }

    void ResetAirJumps()
    {
        // If grounded, reset air jumps
        if (grounded || wallGrounded)
        {
            remainingAirJumps = maxAirJumps;
        }
    }

    void LateUpdate()
    {
        Camera();
    }

    void AutoFire()
    {
        // If we are holding down button
        if (fire.IsPressed())
        {
            foreach (GameObject currentGunObject in gunScripts)
            {
                GunScript gunScript = currentGunObject.GetComponent<GunScript>();
                gunScript.FullAutoFire();
            }
        }
    }
    void FixedUpdate()
    {
        AutoFire();

        Move();

        ResetAirJumps();

        UpdateCrouchingThings();

        WalkingAnimAndMoveForce();

        CrouchingDownForce();

        // Speedometer Text
        speedText.text = "Current Speed: " + Mathf.RoundToInt(new Vector3(currentXSpeed, currentYSpeed, currentZSpeed).magnitude) + "m/s";
        // Apply Gravity Force
        if (wallGrounded)
        {
            rig.AddForce(Vector3.down * gravityForce * gravityMultiplier, ForceMode.Acceleration);
            // Reapply the movement adjustment to handle held keys
            moveValue = AdjustInputForWallRun(moveValue);
        }
        else
            rig.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);

        if (grounded)
        {
            allowGroundSlam = false;
        }
    }

    void CrouchingDownForce()
    {
        if (crouching && !grounded || crouching && !wallGrounded)
        {
            rig.AddForce(Vector3.down * crouchDownForce, ForceMode.Acceleration);
        }
    }

    void WalkingAnimAndMoveForce()
    {
        Vector3 horizontalVelocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);

        bool isMoving = horizontalVelocity.magnitude > movementThreshold;

        camAnim.SetBool("isWalking", isMoving);

        if (isMoving && horizontalVelocity.magnitude < absoluteMaxSpeed)
        {
            moveForce += 0.02f * moveForceIncreasePerSecond;
        }
        else if (!isMoving)
        {
            moveForce = defaultMoveForce;
        }
    }

    void UpdateCrouchingThings()
    {
        cameraContainer.transform.localPosition = new Vector3(0, curCamHeight, 0);
        capsuleCollider.height = curColHeight;
        capsuleCollider.center = Vector3.up * curColCenter;
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Store look value
        deltaMouseValue = context.ReadValue<Vector2>();
    }

    void Camera()
    {
        // Rotate the Camera
        curCameraX += deltaMouseValue.y * lookSenseToUse;
        curCameraX = Mathf.Clamp(curCameraX, minX, maxX);

        cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

        transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Vector2 inputValue = context.ReadValue<Vector2>();
            moveValue = AdjustInputForWallRun(inputValue);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            moveValue = Vector2.zero;
        }
    }

    private Vector2 AdjustInputForWallRun(Vector2 inputValue)
    {
        if (wallGrounded)
        {
            Vector2 adjustedInput = inputValue;

            if (isWallOnRight)
            {
                adjustedInput = new Vector2(Mathf.Clamp(inputValue.x, -1, 0), inputValue.y);
            }
            else if (isWallOnLeft)
            {
                adjustedInput = new Vector2(Mathf.Clamp(inputValue.x, 0, 1), inputValue.y);
            }

            // Normalize to avoid slowing down diagonally, but preserve raw forward input (y)
            return new Vector2(adjustedInput.x, Mathf.Max(adjustedInput.y, inputValue.y)).normalized;
        }

        return inputValue;
    }

    void Move()
    {
        currentYSpeed = rig.velocity.y;
        currentXSpeed = rig.velocity.x;
        currentZSpeed = rig.velocity.z;
        // Define player movement
        Vector3 move = transform.forward * moveValue.y + transform.right * moveValue.x;
        // Multiply Move Acceleration
        move *= moveForce;
        // Add the force
        if (wallGrounded)
            rig.AddForce(move * moveMultiplier, ForceMode.Acceleration);
        else
            rig.AddForce(move, ForceMode.Acceleration);           
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        // If this is the frame we jump
        if (context.phase == InputActionPhase.Performed)
        {
            jumpHeld = true;
            // If we are grounded, jump like normal
            if (grounded || (wallGrounded && !(isWallInFront || isWallInBack || isWallOnRight || isWallOnLeft)))
            {
                Jump();
            }
            else if (isWallInFront && wallGrounded)
            {
                rig.AddForce((Vector3.up + -transform.forward) * jumpForce, ForceMode.Impulse);
                camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);
            }
            else if (isWallInBack && wallGrounded)
            {
                rig.AddForce((Vector3.up + transform.forward) * jumpForce, ForceMode.Impulse);
                camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);     
            }
            else if (isWallOnRight)
            {
                rig.AddForce((Vector3.up + -transform.right) * jumpForce, ForceMode.Impulse);
                camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);      
            }
            else if (isWallOnLeft)
            {
                rig.AddForce((Vector3.up + transform.right) * jumpForce, ForceMode.Impulse);
                camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);                   
            }
            // If we jump while not grounded. Jump and remove one AirJump
            else if (remainingAirJumps >= 1)
            {
                rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
                Jump();
                remainingAirJumps -= 1;
            }
        }
        if (context.phase == InputActionPhase.Canceled)
            jumpHeld = false;
    }

    void Jump()
    {
        rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);
    }

    void OnCollisionEnter(Collision other)
    {
        CheckIfGrounded(other);
        CheckIfWallGrounded(other);

        HoldingJumpButton();

        LandingAndGroundSlam(other);
        WallRunningAnims();
    }

    void WallRunningAnims()
    {
        // If Wallgrounded now but not last frame and not already Wallrunning
        if (wallGrounded && !wasWallGrounded && !grounded)
        {
            if (isWallOnRight)
            {
                Debug.Log("Wall Run Right In");
                camAnim.Play("Wall Run Right In", camAnim.GetLayerIndex("Wall Running Layer"), 0.0f);
            }
            else if (isWallOnLeft)
            {
                Debug.Log("Wall Run Left In");
                camAnim.Play("Wall Run Left In", camAnim.GetLayerIndex("Wall Running Layer"), 0.0f);
            }

            wasWallGrounded = wallGrounded;
        }
        // If was Wallgrounded but not anymore
        else if (!wallGrounded)
        {   
            if (isWallOnRight)
            {
                Debug.Log("Wall Run Right Out");
                camAnim.SetTrigger("wallRunRightOut");
                isWallOnRight = false;
            }
            else if (isWallOnLeft)
            {
                Debug.Log("Wall Run Left Out");
                camAnim.SetTrigger("wallRunLeftOut");
                isWallOnLeft = false;
            }
            else if (isWallInFront)
            {
                isWallInFront = false;
            }
            else if (isWallInBack)
            {
                isWallInBack = false;
            }
            wasWallGrounded = wallGrounded;
        }
    }
    void CheckIfWallGrounded(Collision other)
    {
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);

        foreach (ContactPoint contactPoint in contactPoints)
        {
            // Calculate the angle between the collision normal and the ground
            float groundAngle = Vector3.Angle(contactPoint.normal, Vector3.up);

            // Check if the angle is between 85 and 95 degrees
            if (groundAngle >= wallAngleMinMax.x && groundAngle <= wallAngleMinMax.y)
            {
                Vector3 normal = contactPoint.normal;

                // Dot product to detect collision side
                float dotForward = Vector3.Dot(normal, transform.forward);
                float dotRight = Vector3.Dot(normal, transform.right);
                float dotBack = Vector3.Dot(normal, -transform.forward);
                float dotLeft = Vector3.Dot(normal, -transform.right);

                if (dotForward > 0.5f)
                {
                    isWallInBack = true;
                    wallColliders.Add(other.collider);
                    break;
                }
                else if (dotRight > 0.5f)
                {
                    isWallOnLeft = true;
                    wallColliders.Add(other.collider);
                    break;
                }
                else if (dotBack > 0.5f)
                {
                    isWallInFront = true;
                    wallColliders.Add(other.collider);
                    break;
                }
                else if (dotLeft > 0.5f)
                {
                    isWallOnRight = true;
                    wallColliders.Add(other.collider);
                    break;
                }
            }
        }
        wallGrounded = wallColliders.Any();
    }

    void HoldingJumpButton()
    {
        if (jumpHeld && grounded || jumpHeld && wallGrounded)
        {
            Jump();
        }
    }

    void LandingAndGroundSlam(Collision other)
    {
        if (grounded && !wasGrounded) // Grounded now but not last frame
        {
            camAnim.Play("Land", camAnim.GetLayerIndex("Land Layer"), 0.0f);

            if (crouching && allowGroundSlam)
            {
                Instantiate(groundSlamParticleSystem, other.GetContact(0).point, Quaternion.identity);

                if (Time.time - lastGroundSlamTime <= timeToResetCrouchForces)
                {
                    crouchDownForce += crouchForcesAddPerSlam;
                    crouchDownForce = Mathf.Clamp(crouchDownForce, 0, maxCrouchDownForce);
                    groundSlamUpForce += crouchForcesAddPerSlam;
                    groundSlamUpForce = Mathf.Clamp(groundSlamUpForce, 0, maxGroundSlamUpForce);
                }
                else
                {
                    crouchDownForce = defaultCrouchDownForce;
                    groundSlamUpForce = defaultGroundSlamUpForce;
                }

                lastGroundSlamTime = Time.time;

                if (moveValue.x > 0)
                {
                    Debug.Log("Right Slam");
                    camAnim.Play("Ground Slam Right", camAnim.GetLayerIndex("Slam Layer"), 0.0f);
                    rig.AddForce(other.GetContact(0).normal * groundSlamUpForce, ForceMode.Impulse);
                    allowGroundSlam = false;
                }
                else if (moveValue.x < 0)
                {
                    Debug.Log("Left Slam");
                    camAnim.Play("Ground Slam Left", camAnim.GetLayerIndex("Slam Layer"), 0.0f);
                    rig.AddForce(other.GetContact(0).normal * groundSlamUpForce, ForceMode.Impulse);
                    allowGroundSlam = false;
                }
                else
                {
                    Debug.Log("Middle Slam");
                    camAnim.Play("Ground Slam Middle", camAnim.GetLayerIndex("Slam Layer"), 0.0f);
                    rig.AddForce(other.GetContact(0).normal * groundSlamUpForce, ForceMode.Impulse);
                    allowGroundSlam = false;
                }
                if (groundSlamAction == GroundSlamAction.Explode)
                    Algorithms.Explode(transform.position, groundSlamExplodeRadius, groundSlamExplodeForce, groundSlamUpForce);
                else
                    Algorithms.Implode(transform.position, groundSlamImplodeRadius, groundSlamImplodeForce);
            }
        }
        wasGrounded = grounded;
    }

    void CheckIfGrounded(Collision other)
    {
        // Define Array
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);
        // Transform.pos.y for World Space. curColCenter as col offset. Half of height is distance from center to feet
        var feetYLevel = (transform.position.y + curColCenter) - curColHeight / 2;
        // Check if any contact point is close to feetYLevel
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Mathf.Abs(contactPoints[i].point.y - feetYLevel) < feetTolerance)
            {
                groundColiders.Add(other.collider);
                break;
            }
        }
        grounded = groundColiders.Any();
    }

    void OnCollisionExit(Collision other)
    {
        // Check if Still Grounded
        groundColiders.Remove(other.collider);
        grounded = groundColiders.Any();
        wasGrounded = grounded;
        // Check if Still WallGrounded
        wallColliders.Remove(other.collider);
        wallGrounded = wallColliders.Any();

        WallRunningAnims();
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        // If we crouch on this frame, crouching true. 
        if (context.phase == InputActionPhase.Performed)
        {
            if (!grounded) 
                allowGroundSlam = true;

            if (crouchType == CrouchType.Hold)
            {
                Crouch();
            }
            else
            {
                if (!crouching)
                {
                    Crouch();
                }
                else
                {
                    UnCrouch();
                }
            }
        }
        if (context.phase == InputActionPhase.Canceled && crouchType == CrouchType.Hold)
        {
            UnCrouch();
        }
    }

    void Crouch()
    {
        StopAllCoroutines();
        StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, crouchTargetCamHeight, crouchCurve, crouchTransitionTime));
        StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, crouchTargetColHeight, crouchCurve, crouchTransitionTime));
        StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, crouchTargetColCenter, crouchCurve, crouchTransitionTime));
        crouching = true;
    }

    void UnCrouch()
    {
        StopAllCoroutines();
        StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, defaultCamHeight, crouchCurve, crouchTransitionTime));
        StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, defaultColHeight, crouchCurve, crouchTransitionTime));
        StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, defaultColCenter, crouchCurve, crouchTransitionTime));
        crouching = false;
    }

    public void OnReloadInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            foreach (GameObject currentGunObject in gunScripts)
            {
                GunScript gunScript = currentGunObject.GetComponent<GunScript>();
                gunScript.Reload();
            }
        }
    }

    public void OnFireInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            foreach (GameObject currentGunObject in gunScripts)
            {
                GunScript gunScript = currentGunObject.GetComponent<GunScript>();
                gunScript.SemiAutoFire();
            }
        }
    }

    public void OnExitInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            Application.Quit();
    }

    public void DoFireAnim(int size)
    {
        // Play corresponding firing animation
        switch (size)
        {
            case 1:
                camAnim.Play("Small Fire", camAnim.GetLayerIndex("Small Fire"), 0.0f);
                break;
            case 2:
                camAnim.Play("Medium Fire", camAnim.GetLayerIndex("Medium Fire"), 0.0f);
                break;
            case 3:
                camAnim.Play("Big Fire", camAnim.GetLayerIndex("Big Fire"), 0.0f);
                break;
        }
    }

    public void OnGrappleInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {

        }
    }

    bool CamAnimReadyToAnim(string layerName)
    {
        // Return true if no other anim playing on layer
        int layerIndex = camAnim.GetLayerIndex(layerName);
        return camAnim.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime > 1 && !camAnim.IsInTransition(layerIndex);
    }
}
