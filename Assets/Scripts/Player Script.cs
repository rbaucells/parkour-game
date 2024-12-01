using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;
using UnityEditor.EditorTools;

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

    [Header("Camera")]
    public float controllerLookSens;
    public float mouseLookSens;

    private float lookSenseToUse;

    [Tooltip("Up and Down Rotation")] public float maxX;
    [Tooltip("Up and Down Rotation")] public float minX;

    [HideInInspector] public float curCameraX; // Up and Down

    private Vector2 deltaMouseValue;

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

    [Header("Ground Check")]
    private bool grounded = false;
    private bool wasGrounded;

    private ISet<Collider> colliders = new HashSet<Collider>();
    private const float feetTolerance = 0.3f;

    [Header("Jumping")]

    public float jumpForce;
    public float gravityForce;

    public int maxAirJumps;
    private int remainingAirJumps;

    [Header("Crouching")]
    public CrouchType crouchType;
    private string crouchString;

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

    [Header("Ground Slam")]
    public GroundSlamAction groundSlamAction;
    private string groundSlamActionString;

    private float defaultGroundSlamUpForce;
    public float groundSlamUpForce;
    public float maxGroundSlamUpForce;

    private bool allowGroundSlam;
    private float lastGroundSlamTime;
    [Header("Explode")]
    public float groundSlamExplodeRadius;
    public float groundSlamExplodeForce;
    public float groundSlamExplodeUpForce;
    [Header("Implode")]
    public float groundSlamImplodeRadius;
    public float groundSlamImplodeForce;

    [Header("References")]
    public TextMeshProUGUI speedText;

    private CapsuleCollider capsuleCollider;
    private Transform cameraContainer;
    private Animator camAnim;
    private Rigidbody rig;
    [Header("Weapons")]
    private GameObject[] gunScripts;
    private PlayerInput playerInput;
    private InputAction fire;
    [Header("Effects")]
    public GameObject groundSlamParticleSystem;

    void Awake()
    {
        InitializeComponents();
        SetDefaults();
        SetCrouchType();
        SetGroundSlamAction();
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

    void SetCrouchType()
    {
        crouchString = crouchType == CrouchType.Hold ? "Hold" : "Toggle";
    }

    void SetGroundSlamAction()
    {
        groundSlamActionString = groundSlamAction == GroundSlamAction.Explode ? "Explode" : "Implode";
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
        if (grounded)
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
        rig.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);

        if (grounded)
        {
            allowGroundSlam = false;
        }
    }

    void CrouchingDownForce()
    {
        if (crouching && !grounded)
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
        // If this is the frame movement key is pressed, Store the value
        if (context.phase == InputActionPhase.Performed)
        {
            moveValue = context.ReadValue<Vector2>();
        }
        // Else reset movement value.
        else if (context.phase == InputActionPhase.Canceled)
        {
            moveValue = Vector2.zero;
        }
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
        rig.AddForce(move, ForceMode.Acceleration);
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        // If this is the frame we jump
        if (context.phase == InputActionPhase.Performed)
        {
            // If we are grounded, jump like normal
            if (grounded)
            {
                rig.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
                camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);
            }
            // If we jump while not grounded. Jump and remove one AirJump
            else if (remainingAirJumps >= 1)
            {
                rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
                rig.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
                camAnim.Play("Jump", camAnim.GetLayerIndex("Jump Layer"), 0.0f);
                remainingAirJumps -= 1;
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        CheckIfGrounded(other);
        LandingAndGroundSlam(other);
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
                if (groundSlamActionString == "Explode")
                    GroundSlamExplode();
                else
                    GroundSlamImplode();
            }
        }
        wasGrounded = grounded;
    }

    void GroundSlamExplode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, groundSlamExplodeRadius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                collider.gameObject.GetComponent<Rigidbody>().AddExplosionForce(groundSlamExplodeForce, transform.position, groundSlamExplodeRadius, groundSlamExplodeUpForce);
            }
        }
    }

    void GroundSlamImplode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, groundSlamImplodeRadius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                collider.gameObject.GetComponent<Rigidbody>().AddForce((transform.position - collider.transform.position).normalized * groundSlamImplodeForce, ForceMode.Impulse);
            }    
        }
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
                colliders.Add(other.collider);
            }
        }
        grounded = colliders.Any();
    }

    void OnCollisionExit(Collision other)
    {
        colliders.Remove(other.collider);
        grounded = colliders.Any();
        wasGrounded = grounded;
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        // If we crouch on this frame, crouching true. 
        if (context.phase == InputActionPhase.Performed)
        {
            if (!grounded)
                allowGroundSlam = true;
            if (crouchString == "Hold")
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
        if (context.phase == InputActionPhase.Canceled && crouchString == "Hold")
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

    public void DoFireAnim(string size)
    {
        // Play corresponding firing animation
        switch (size)
        {
            case "Small":
                camAnim.Play("Small Fire", camAnim.GetLayerIndex("Small Fire"), 0.0f);
                break;
            case "Medium":
                camAnim.Play("Medium Fire", camAnim.GetLayerIndex("Medium Fire"), 0.0f);
                break;
            case "Big":
                camAnim.Play("Big Fire", camAnim.GetLayerIndex("Big Fire"), 0.0f);
                break;
        }
    }

    bool camAnimReadyToAnim(string layerName)
    {
        // Return true if no other anim playing on layer
        int layerIndex = camAnim.GetLayerIndex(layerName);
        return camAnim.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime > 1 && !camAnim.IsInTransition(layerIndex);
    }
}
