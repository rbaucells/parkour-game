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

    public float crouchXMultiplier;

    private IList<Coroutine> crouchingCorutines = new List<Coroutine>();
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

    private bool wallGrounded = false;
    private bool wasWallGrounded;

    private bool isWallOnRight;
    private bool isWallOnLeft;
    private bool isWallInFront;
    private bool isWallInBack;

    private const float wallTolerance = 0.1f; // Tolerance for horizontal proximity to wall

    private bool wallrunning;
    private ISet<Collider> wallColliders = new HashSet<Collider>();

    public float wallRunRightTargetAngleY;
    public float wallRunRightTargetAngleZ;

    public float wallRunLeftTargetAngleY;
    public float wallRunLeftTargetAngleZ;
    
    public float curWallRunAngleY;
    public float curWallRunAngleZ;

    public AnimationCurve wallRunAngleCurve;
    public float wallRunAngleTime;

    private IList<Coroutine> wallRunCoroutines = new List<Coroutine>();

    private Vector3 wallAttractionDireciton;
    //--------------------Dash--------------------//
    [Header("Dash")]
    public float dashForce;
    public float dashDelay;
    public float dashTime;
    public float leaveVelocityMultiplier;

    private float lastDashTime;
    //--------------------Grapple--------------------//
    [Header("Grapple")]
    public float maxGrappleDistance;

    public float damper;
    public float spring;
    //--------------------References--------------------//
    [Header("References")]
    public TextMeshProUGUI speedText;
    private CapsuleCollider capsuleCollider;
    private Transform cameraContainer;
    private Transform weaponsContainer;
    private new Transform camera;
    private Animator camAnim;
    private Rigidbody rig;
    public Transform shootCenter;

    //--------------------Weapons--------------------//
    [Header("Weapons")]
    public GameObject[] gunInventory;
    private GameObject[] gunScripts;
    private PlayerInput playerInput;
    private InputAction fire;
    private bool firing;
    //--------------------Effects--------------------//
    [Header("Effects")]
    public GameObject groundSlamParticleSystem;


    #region Start

    void Awake() // Called Before First Frame
    {
        InitializeComponents();
        SetDefaults();
    }
    
    void Start() // Called On First Frame
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Every 3 Seconds, Check for input change
        InvokeRepeating(nameof(CheckIfInputChange), 0.0f, 3f);
        
        int counter = 0;

        GameObject weaponContainer = cameraContainer.transform.GetChild(1).gameObject;

        foreach (GameObject gunObject in gunInventory)
        {
            GameObject curGun = Instantiate(gunObject, weaponContainer.transform);
            curGun.transform.localPosition = curGun.GetComponent<GunScript>().weaponPos[counter];
            counter += 1;
        }

        gunScripts = GameObject.FindGameObjectsWithTag("Gun");
    }

    void InitializeComponents() // Called In Awake(). To Define References
    {
        playerInput = GetComponent<PlayerInput>();
        rig = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        cameraContainer = GameObject.Find("Camera Container").transform;

        weaponsContainer = GameObject.Find("Weapons Holder").transform;

        camera = GameObject.Find("Camera").transform;

        camAnim = cameraContainer.GetComponent<Animator>();

        fire = playerInput.actions["Shoot"];
    }

    void SetDefaults() // Called In Awake(). To Define Default Values
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

    void CheckIfInputChange() // First Called In Start(). Called Every 3 Seconds. To Check Which Input Is Being Used
    {
        if (Gamepad.all.Count > 0)
            lookSenseToUse = controllerLookSens;
        else
            lookSenseToUse = mouseLookSens;
    }

    #endregion

    #region Update

    void FixedUpdate() // Called at Fixed Interval. Frame-Rate Independant
    {
        ApplyGravity();
        Move();
        ResetAirJumps();
        WalkingAnimAndMoveForce();

        GroundedDisableSlam();
        UpdateCrouchingThings();
        UpdateWallRunningThings();
        // Speedometer Text
        speedText.text = "Current Speed: " + Mathf.RoundToInt(new Vector3(currentXSpeed, currentYSpeed, currentZSpeed).magnitude) + "m/s";
    }

    void LateUpdate() // Called After All Other Update Functions
    {
        Camera();
    }

    void ApplyGravity() // Called in Fixed Update(). Applies Gravity to Player.
    {
        if (wallGrounded)
        {
            rig.AddForce(Vector3.down * gravityForce * gravityMultiplier, ForceMode.Acceleration);

            rig.AddForce(wallAttractionForce * wallAttractionDireciton, ForceMode.Acceleration);
        }
        else
            rig.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        if (crouching && !grounded || crouching && !wallGrounded)
        {
            rig.AddForce(Vector3.down * crouchDownForce, ForceMode.Acceleration);
        }
    }

    void Move() // Called In Fixed Update(). Adds Movement Forces
    {
        currentYSpeed = rig.velocity.y;
        currentXSpeed = rig.velocity.x;
        currentZSpeed = rig.velocity.z;
        // Define player movement
        Vector2 adjustedMoveValue = AdjustInputForWallRun(moveValue);
        Vector3 move = transform.forward * adjustedMoveValue.y + transform.right * adjustedMoveValue.x;
        // Multiply Move Acceleration
        // Multiply Move Acceleration
        move *= moveForce;

        if (wallGrounded)
            rig.AddForce(move * moveMultiplier, ForceMode.Acceleration);
        else if (crouching)
        {
            rig.AddForce(new Vector3(move.x * crouchXMultiplier, 0, move.z), ForceMode.Acceleration);   
        } 
        else
            rig.AddForce(move, ForceMode.Acceleration); 
    }

    void ResetAirJumps() // Called In Fixed Update(). To Reset AirJumps when Grounded
    {
        if (grounded || wallGrounded)
        {
            remainingAirJumps = maxAirJumps;
        }
    }

    void WalkingAnimAndMoveForce() // Called In Fixed Update(). Plays Walking Anim And Increases Move Force Over Time.
    {
        Vector3 horizontalVelocity = new(rig.velocity.x, 0, rig.velocity.z);

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

    void GroundedDisableSlam() // Called In Fixed Update(). Disables Ground Slam While Grounded
    {
        if (grounded || wallGrounded)
        {
            allowGroundSlam = false;
        }   
    }

    void UpdateCrouchingThings() // Called In Fixed Update(). Sets Values Used In Crouching Coroutines
    {
        cameraContainer.transform.localPosition = Vector3.up * curCamHeight;
        capsuleCollider.height = curColHeight;
        capsuleCollider.center = Vector3.up * curColCenter;
    }

    void UpdateWallRunningThings()
    {
        weaponsContainer.localEulerAngles = new Vector3(weaponsContainer.eulerAngles.x,curWallRunAngleY,curWallRunAngleZ);
        camera.localEulerAngles = new(camera.eulerAngles.x,curWallRunAngleY,curWallRunAngleZ);
    }
    
    void Camera() // Called In Late Update(). Rotates Camera and Player as Needed
    {
        // Rotate the Camera
        curCameraX += deltaMouseValue.y * lookSenseToUse;
        curCameraX = Mathf.Clamp(curCameraX, minX, maxX);

        cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

        transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
    }

    #endregion

    #region Input

    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Stores Delta Look Value
        deltaMouseValue = context.ReadValue<Vector2>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            moveValue = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            moveValue = Vector2.zero;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        // If this is the frame we jump
        if (context.phase == InputActionPhase.Performed)
        {
            jumpHeld = true;
            Jump();
        }
        if (context.phase == InputActionPhase.Canceled)
            jumpHeld = false;
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
    
    public void OnExitInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            Application.Quit();
    }

    public void OnGrappleInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {

        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            
        }
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && lastDashTime + dashDelay <= Time.time)
        {
            lastDashTime = Time.time;
            StartCoroutine(Dash());
        }
    }

    #endregion
    
    #region Jumping/Collision

    void OnCollisionEnter(Collision other)
    {
        // Ground Checks
        CheckIfGrounded(other);
        CheckIfWallGrounded(other);

        HoldingJumpButton();

        // Effects
        LandingAndGroundSlam(other);
        WallRunningAnims();
    }

    void CheckIfGrounded(Collision other) // Called In OnCollisionEnter(). Checks If Any Collision Points at FeetYLevel
    {
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);
        var feetYLevel = (transform.position.y + curColCenter) - curColHeight / 2;
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

    void CheckIfWallGrounded(Collision other) // Called In OnCollisionEnter(). Checks If Wall Angle is Good. Then Checks Which Side Collision Occured
    {
        var contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(contactPoints);

        foreach (ContactPoint contactPoint in contactPoints)
        {
            float groundAngle = Vector3.Angle(contactPoint.normal, Vector3.up);
            if (groundAngle >= wallAngleMinMax.x && groundAngle <= wallAngleMinMax.y)
            {
                Vector3 normal = contactPoint.normal;

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

                wallAttractionDireciton = -normal.normalized;
            }
        }
        wallGrounded = wallColliders.Any();
    }

    void HoldingJumpButton() // Called In OnCollisionEnter(). Jumps if Button Is Held
    {
        if (jumpHeld && grounded)
        {
            Jump();
        }
    }
        
    void LandingAndGroundSlam(Collision other) // Called In OnCollisionEnter(). If Not Grounded Before, But Grounded Now. Play Animations and GroundSlam
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

    void WallRunningAnims() // Called In OnCollisionEnter() and OnCollisionExit(). If Not WallGrounded Before, But WallGrounded now. Play Anim. Vice Versa
    {
        if (wallGrounded && !wasWallGrounded && !grounded)
        {
            if (isWallOnRight)
            {
                Debug.Log("Wall Run Right In");
                WallRunRightIn();
            }
            else if (isWallOnLeft)
            {
                Debug.Log("Wall Run Left In");
                WallRunLeftIn();
            }
        }
        else if (!wallGrounded && wasWallGrounded)
        {   
            if (isWallOnRight)
            {
                Debug.Log("Wall Run Right Out");
                WallRunRightOut();
                isWallOnRight = false;
            }
            else if (isWallOnLeft)
            {
                Debug.Log("Wall Run Left Out");
                WallRunLeftOut();
                isWallOnLeft = false;
            }
            else if (isWallInFront)
            {
                Debug.Log("Wall Run Front Out");
                isWallInFront = false;
            }
            else if (isWallInBack)
            {
                Debug.Log("Wall Run Back Out");
                isWallInBack = false;
            }
        }
        wasWallGrounded = wallGrounded;
    }

    void Jump() // Called in OnJumpInput() On First Frame. Determines Direction to Jump In
    {
        if (grounded || (wallGrounded && !(isWallInFront || isWallInBack || isWallOnRight || isWallOnLeft)))
        {
            AddJumpForce(Vector3.up);
        }
        else if (isWallInFront && wallGrounded)
        {
            AddJumpForce(Vector3.up + -transform.forward);
        }
        else if (isWallInBack && wallGrounded)
        {
            AddJumpForce(Vector3.up + transform.forward); 
        }
        else if (isWallOnRight)
        {
            AddJumpForce(Vector3.up + -transform.right);
        }
        else if (isWallOnLeft)
        {
            AddJumpForce(Vector3.up + transform.right);         
        }
        else if (remainingAirJumps >= 1)
        {
            rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
            AddJumpForce(Vector3.up);
            remainingAirJumps -= 1;
        }
    }

    void AddJumpForce(Vector3 direction) // Called In Jump(). Adds A Force In direction
    {
        rig.AddForce(direction * jumpForce, ForceMode.Impulse);
        camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);
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

    #endregion

    #region OtherFunctions

    private IEnumerator Dash() // Called In OnDashInput(). Adds Force, Waits dashTime. Then Resets player velocity to starting velocity * leaveVelocityMultiplier
    {
        Vector3 startVelocity = new Vector3(currentXSpeed, 0, currentZSpeed);
        rig.AddForce(cameraContainer.forward * dashForce, ForceMode.Impulse);
        camAnim.Play("Dash", camAnim.GetLayerIndex("Dash Layer"), 0.0f);
        yield return new WaitForSeconds(dashTime);
        rig.velocity = startVelocity * leaveVelocityMultiplier;
    }

    private void Crouch() // Called In OnCrouchInput(). Starts A Bunch Of Coroutines to Move the Player
    {
        Debug.Log("Crouch");
        StopCrouchingCoroutines();
        crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, crouchTargetCamHeight, crouchCurve, crouchTransitionTime)));
        crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, crouchTargetColCenter, crouchCurve, crouchTransitionTime)));
        crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, crouchTargetColHeight, crouchCurve, crouchTransitionTime)));
        crouching = true;
    }

    private void UnCrouch() // Called In OnCrouchInput(). Starts A Bunch Of Coroutines to Move the Player
    {
        Debug.Log("Un-Crouch");
        StopCrouchingCoroutines();
        crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, defaultCamHeight, crouchCurve, crouchTransitionTime)));
        crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, defaultColCenter, crouchCurve, crouchTransitionTime)));
        crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, defaultColHeight, crouchCurve, crouchTransitionTime)));
        crouching = false;
    }

    void StopCrouchingCoroutines()
    {
        Debug.Log("Stop Crouch Routine");

        foreach(var coroutine in crouchingCorutines)
        {
            StopCoroutine(coroutine);
        }

        crouchingCorutines.Clear();
    }

    public void DoFireAnim(int size) // Called In GunScript.Shoot(). Plays Screen Shake Animation
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

    void WallRunRightIn()
    {
        // StopWallRunCoroutines();
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleY", curWallRunAngleY, wallRunRightTargetAngleY, wallRunAngleCurve, wallRunAngleTime)));
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleZ", curWallRunAngleZ, wallRunRightTargetAngleZ, wallRunAngleCurve, wallRunAngleTime)));
        camAnim.Play("Right In", camAnim.GetLayerIndex("Wall Running Layer"), 0.0f);
    }

    void WallRunRightOut()
    {
        // StopWallRunCoroutines();
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleY", curWallRunAngleY, 0, wallRunAngleCurve, wallRunAngleTime)));
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleZ", curWallRunAngleZ, 0, wallRunAngleCurve, wallRunAngleTime)));
        camAnim.SetBool("Right Out", true);
        StartCoroutine(TurnOffWallRunTransitionBool());
    }

    void WallRunLeftIn()
    {
        // StopWallRunCoroutines();
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleY", curWallRunAngleY, wallRunLeftTargetAngleY, wallRunAngleCurve, wallRunAngleTime)));
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleZ", curWallRunAngleZ, wallRunLeftTargetAngleZ, wallRunAngleCurve, wallRunAngleTime)));
        camAnim.Play("Left In", camAnim.GetLayerIndex("Wall Running Layer"), 0.0f);
    }

    void WallRunLeftOut()
    {
        // StopWallRunCoroutines();
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleY", curWallRunAngleY, 0, wallRunAngleCurve, wallRunAngleTime)));
        // wallRunCoroutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curWallRunAngleZ", curWallRunAngleZ, 0, wallRunAngleCurve, wallRunAngleTime)));
        camAnim.SetBool("Left Out", true);
        StartCoroutine(TurnOffWallRunTransitionBool());
    }

    IEnumerator TurnOffWallRunTransitionBool()
    {
        yield return null;
        camAnim.SetBool("Left Out", false);
        camAnim.SetBool("Right Out", false);
    }

    void StopWallRunCoroutines()
    {
        Debug.Log("Stop Wall Run Routines");
        foreach(var coroutine in wallRunCoroutines)
        {
            StopCoroutine(coroutine);
        }

        wallRunCoroutines.Clear();
    }

    #endregion

    #region Utilities

    private bool CamAnimReadyToAnim(string layerName) // Returns True If No Animations Running on Layer
    {
        int layerIndex = camAnim.GetLayerIndex(layerName);
        return camAnim.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime > 1 && !camAnim.IsInTransition(layerIndex);
    }

    private Vector2 AdjustInputForWallRun(Vector2 inputValue) // Disables Movement in Direction of Wall While WallRunning.
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

    #endregion
}
