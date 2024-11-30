using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;

public class PlayerScript : MonoBehaviour
{
    public enum CrouchType
    {
        Hold,
        Toggle
    };

    [Header("Camera")]
    private Vector2 lookValue;
    [HideInInspector]
    public float curCameraX;

    public float minX;
    public float maxX;

    private float defaultMinX;
    private float defaultMaxX;

    private float lookSens;
    public float controllerLookSens;
    public float mouseLookSens;

    public Transform cameraContainer;

    public Animator camAnim;

    [Header("Movement")]
    public float moveAccel;
    
    private Vector2 moveValue;
    private Rigidbody rig;

    private float currentSpeed;

    private float currentXSpeed;
    private float currentZSpeed;

    public float gravity;

    private const float movementThreshold = 0.1f;

    public float speedIncreasePerSecond;
    public float absoluteMaxSpeed;

    private float defaultMoveAccel;

    [Header("Jumping")]
    public float jumpForce;

    public float maxAirJumps;
    private float remainingAirJumps;
    public bool grounded = false;

    [Header("Crouching")]
    private float defaultCamHeight;
    private float defaultColHeight;
    private float defaultColRadius;
    private float defaultColCenter;

    public float targetCamHeight;
    public float targetColHeight;
    public float targetColRadius;
    public float targetColCenter;

    public float crouchTransitionTime;

    [HideInInspector]
    public float curCamHeight;
    [HideInInspector]
    public float curColHeight;
    [HideInInspector]
    public float curColRadius;
    [HideInInspector]
    public float curColCenter;

    public AnimationCurve crouchCurve;

    public CrouchType crouchType;
    private string crouchString;

    private bool crouching;

    public float crouchDownForce;
    private float defaultCrouchDownForce;
    public float crouchForcesAddPerSlam;

    public float groundSlamUpForce;

    private bool disableGroundPound;
    private float groundSlamTime;
    public float timeToResetCrouchForces;
    private float defaultGroundSlamUpFore;

    public float maxGroundSlamUpForce;
    public float maxCrouchDownForce;

    [Header("Other Things")]
    public TextMeshProUGUI speedText;

    public CapsuleCollider capsuleCollider;

    [Header("Weapons")]
    private GameObject[] gunScripts;
    private PlayerInput playerInput;
    private InputAction fire;
    [Header("Effects")]
    public GameObject groundSlamParticleSystem;
    


    void Awake()
    {
        // Get components
        playerInput = GetComponent<PlayerInput>();
        rig = GetComponent<Rigidbody>();
        // Define fire InputAction
        fire = playerInput.actions["Shoot"];

        gunScripts = GameObject.FindGameObjectsWithTag("Gun");

        defaultCamHeight = cameraContainer.transform.localPosition.y;
        defaultColHeight = capsuleCollider.height;
        defaultColRadius = capsuleCollider.radius;
        defaultColCenter = capsuleCollider.center.y;

        curCamHeight = defaultCamHeight;
        curColHeight = defaultColHeight;
        curColRadius = defaultColRadius;
        curColCenter = defaultColCenter;

        SetCrouchType();

        defaultMoveAccel = moveAccel;
        defaultCrouchDownForce = crouchDownForce;
        defaultGroundSlamUpFore = groundSlamUpForce;
    }
    void SetCrouchType()
    {
        switch (crouchType)
        {
            case CrouchType.Hold:
                crouchString = "Hold";
                break;
            case CrouchType.Toggle:
                crouchString = "Toggle";
                break;
        }
    }

    void Start()
    {
        // Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;

        InvokeRepeating(nameof(CheckIfInputChange), 0.0f, 3f);
    }

    void CheckIfInputChange()
    {
        if (Gamepad.all.Count > 0)
            lookSens = controllerLookSens;
        else
            lookSens = mouseLookSens;
    }

    void ResetJumps()
    {
        // If grounded, reset air jumps
        if (grounded)
        {
            remainingAirJumps = maxAirJumps;
        }
    }

    void LateUpdate()
    {
        // Move the Camera
        CameraLook();
    }

    void FixedUpdate()
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
        // Move the player
        Move();
        // Update Speedometer Text
        speedText.text = "Current Speed: " + Mathf.RoundToInt(currentSpeed) + "m/s";
        // Check to reset Jumps
        ResetJumps();
        rig.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        cameraContainer.transform.localPosition = new Vector3(0, curCamHeight, 0);
        capsuleCollider.height = curColHeight;
        capsuleCollider.radius = curColRadius;
        capsuleCollider.center = Vector3.up * curColCenter;

        Vector3 horizontalVelocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);

        bool isMoving = horizontalVelocity.magnitude > movementThreshold;

        camAnim.SetBool("isWalking", isMoving);

        if (isMoving && horizontalVelocity.magnitude < absoluteMaxSpeed)
        {
            moveAccel += 0.02f * speedIncreasePerSecond;
        }
        else if (!isMoving)
        {
            moveAccel = defaultMoveAccel;
        }

        if (crouching && !grounded)
        {
            rig.AddForce(Vector3.down * crouchDownForce, ForceMode.Acceleration);
        }

        grounded = colliders.Any();
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Store look value
        lookValue = context.ReadValue<Vector2>();
    }

    void CameraLook()
    {
        // Rotate the Camera
        curCameraX += lookValue.y * lookSens;
        curCameraX = Mathf.Clamp(curCameraX, minX, maxX);

        cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

        transform.eulerAngles += new Vector3(0, lookValue.x * lookSens, 0);
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
        currentSpeed = rig.velocity.magnitude;
        currentXSpeed = rig.velocity.x;
        currentZSpeed = rig.velocity.z;
        // Define player movement
        Vector3 move = transform.forward * moveValue.y + transform.right * moveValue.x;
        // Multiply Move Acceleration
        move *= moveAccel;
        // Add the force
        rig.AddForce(move, ForceMode.Acceleration);
    }

    bool ReadyToAnim(string layerName)
    {
        // Return true if no other anim playing
        int layerIndex = camAnim.GetLayerIndex(layerName);
        return camAnim.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime > 1 && !camAnim.IsInTransition(layerIndex);
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        // If this is the frame we jump
        if (context.phase == InputActionPhase.Performed)
        {
            // If we are grounded, jump like normal
            if (grounded)
            {
                Debug.Log("Jumped");
                rig.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse); 
                camAnim.Play("Jump", StringToIndex("Jump Layer"), 0.0f);
            }
            // If we jump while not grounded. Jump and remove one AirJump
            else if (remainingAirJumps >= 1)
            {
                Debug.Log("Airjumped");
                rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
                rig.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);

                remainingAirJumps -= 1;
            }
        }
    }

    void OnCollisionEnter(Collision other) 
    {
        DoGroundedThings(other);
        LandingThings(other);
    }
    
    bool allowGroundSlam;
    bool lastFrameGrounded;

    void LandingThings(Collision other)
    {
        if (!lastFrameGrounded && grounded)
        {
            lastFrameGrounded = true;

            Debug.Log("Play land anim");
            camAnim.Play("Land", StringToIndex("Land Layer"), 0.0f);
            if (crouching && allowGroundSlam)
            {
                Instantiate(groundSlamParticleSystem, other.GetContact(0).point, Quaternion.identity);
                if (Time.time - groundSlamTime <= timeToResetCrouchForces)
                {
                    crouchDownForce += crouchForcesAddPerSlam;
                    crouchDownForce = Mathf.Clamp(crouchDownForce, 0, maxCrouchDownForce);
                    groundSlamUpForce += crouchForcesAddPerSlam;
                    groundSlamUpForce = Mathf.Clamp(groundSlamUpForce, 0, maxGroundSlamUpForce);
                }
                else
                {
                    crouchDownForce = defaultCrouchDownForce;
                    groundSlamUpForce = defaultGroundSlamUpFore;
                }
                groundSlamTime = Time.time;

                if (moveValue.x > 0)
                {
                    Debug.Log("Right Slam");
                    camAnim.Play("Ground Slam Right", StringToIndex("Slam Layer"), 0.0f);
                    rig.AddForce(Vector3.up * groundSlamUpForce, ForceMode.Impulse);
                    allowGroundSlam = false;    
                }
                else if (moveValue.x < 0)
                {
                    Debug.Log("Left Slam");
                    camAnim.Play("Ground Slam Left", StringToIndex("Slam Layer"), 0.0f);
                    rig.AddForce(Vector3.up * groundSlamUpForce, ForceMode.Impulse);  
                    allowGroundSlam = false;    
                }
                else
                {
                    Debug.Log("Middle Slam");
                    camAnim.Play("Ground Slam Middle", StringToIndex("Slam Layer"), 0.0f);
                    rig.AddForce(Vector3.up * groundSlamUpForce, ForceMode.Impulse); 
                    allowGroundSlam = false;    
                }
            }
        }
        else
            lastFrameGrounded = !lastFrameGrounded && grounded;
    }

    void DoGroundedThings(Collision other)
    {
        // array
        var contactPoints =  new ContactPoint[other.contactCount];
        // contact points
        other.GetContacts(contactPoints);

        var feetYLevel = (transform.position.y + curColCenter) - curColHeight/2;
        Debug.Log(feetYLevel);
        // loop
        for(int i = 0; i < contactPoints.Length; i++) {

            Debug.Log("y: " + contactPoints[i].point.y + ", level: " + feetYLevel + ", tolerance: " + feetTolerance);

            // check position
            if ( Mathf.Abs(contactPoints[i].point.y - feetYLevel) < feetTolerance)
            {
                colliders.Add(other.collider);
            }
        }
    }

    ISet<Collider> colliders = new HashSet<Collider>();
    float feetTolerance = 0.3f;

    void OnCollisionExit(Collision other) 
    {
        colliders.Remove(other.collider);
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        // If we crouch on this frame, crouching true. 
        if (context.phase == InputActionPhase.Performed)
        {
            allowGroundSlam = true;    
            if (crouchString == "Hold")
            {
                StopAllCoroutines();
                StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, targetCamHeight, crouchCurve, crouchTransitionTime));
                StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, targetColHeight, crouchCurve, crouchTransitionTime));
                StartCoroutine(Algorithms.CurveLerp(this, "curColRadius", curColRadius, targetColRadius, crouchCurve, crouchTransitionTime));
                StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, targetColCenter, crouchCurve, crouchTransitionTime));
                crouching = true;
            }
            else
            {
                if (!crouching)
                {
                    StopAllCoroutines();
                    StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, targetCamHeight, crouchCurve, crouchTransitionTime));
                    StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, targetColHeight, crouchCurve, crouchTransitionTime));
                    StartCoroutine(Algorithms.CurveLerp(this, "curColRadius", curColRadius, targetColRadius, crouchCurve, crouchTransitionTime));
                    StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, targetColCenter, crouchCurve, crouchTransitionTime));
                    crouching = true;
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, defaultCamHeight, crouchCurve, crouchTransitionTime));
                    StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, defaultColHeight, crouchCurve, crouchTransitionTime));
                    StartCoroutine(Algorithms.CurveLerp(this, "curColRadius", curColRadius, defaultColRadius, crouchCurve, crouchTransitionTime));
                    StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, defaultColCenter, crouchCurve, crouchTransitionTime));
                    crouching = false;
                }
            }
        }
        if (context.phase == InputActionPhase.Canceled && crouchString == "Hold")
        {
            StopAllCoroutines();
            StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, defaultCamHeight, crouchCurve, crouchTransitionTime));
            StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, defaultColHeight, crouchCurve, crouchTransitionTime));
            StartCoroutine(Algorithms.CurveLerp(this, "curColRadius", curColRadius, defaultColRadius, crouchCurve, crouchTransitionTime));
            StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, defaultColCenter, crouchCurve, crouchTransitionTime));
            crouching = false;
        }
    }

    public void OnReloadInput(InputAction.CallbackContext context)
    {
        // If this is the frame we reload
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
        // If this is the frame we shoot
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
        Debug.Log("Going to do FireAnim");
        if (size == "Small")
        {
            Debug.Log("Small Fire Recoil");
            camAnim.Play("Small Fire", StringToIndex("Small Fire"), 0.0f);
        }
        else if (size == "Medium")
        {
            Debug.Log("Medium Fire Recoil");
            camAnim.Play("Medium Fire", StringToIndex("Medium Fire"), 0.0f);
        }
        else if (size == "Big")
        {
            Debug.Log("Big Fire Recoil");
            camAnim.Play("Big Fire", StringToIndex("Big Fire"), 0.0f);
        }
    }

    int StringToIndex(string layer)
    {
        return camAnim.GetLayerIndex(layer);
    }
}
