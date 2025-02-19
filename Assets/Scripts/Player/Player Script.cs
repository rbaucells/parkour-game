// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using TMPro;
// using System.Linq;
// using UnityEditor.EditorTools;
// using System;
// using System.Diagnostics;
// using Debug = UnityEngine.Debug;
// using System.Data.Common;
// using UnityEngine.UIElements;

// public class PlayerScript : MonoBehaviour
// {    
//     public enum CrouchType
//     {
//         Hold,
//         Toggle
//     };

    
//     public enum GroundSlamAction
//     {
//         Explode,
//         Implode
//     };

//     public enum WallSide
//     {
//         Front,
//         Back,
//         Right,
//         Left,
//         None
//     };

//     //--------------------Camera--------------------//
//     [Header("Camera")]
//     public float controllerLookSens;
//     public float mouseLookSens;

//     private float lookSenseToUse;

//     [Tooltip("Up and Down Rotation")] public float maxX;
//     [Tooltip("Up and Down Rotation")] public float minX;

//     [HideInInspector] public float curCameraX; // Up and Down

//     private Vector2 deltaMouseValue;

//     //--------------------Movement--------------------//
//     [Header("Movement")]
//     public float moveForce;
//     private float defaultMoveForce;

//     private Vector2 moveValue; // Current Keyboard/Controller input

//     private float currentXSpeed;
//     private float currentYSpeed;
//     private float currentZSpeed;

//     private const float movementThreshold = 0.1f;

//     public float moveForceIncreasePerSecond;
//     public float absoluteMaxSpeed;

//     //--------------------Ground Check--------------------//
//     [Header("Ground Check")]
//     private bool grounded = false;

//     private ISet<Collider> groundColiders = new HashSet<Collider>();
//     private const float feetTolerance = 0.3f;

//     //--------------------Jumping--------------------//
//     [Header("Jumping")]

//     public float jumpForce;
//     public float gravityForce;

//     private bool jumpHeld = false;
//     public int maxAirJumps;
//     private int remainingAirJumps;

//     public float cayoteTime = 1;

//     private float defaultCayoteTime;
//     //--------------------Crouching--------------------//
//     [Header("Crouching")]
//     public CrouchType crouchType;

//     private bool isCrouching;
//     public float crouchTransitionTime;

//     private float defaultCamHeight;
//     private float defaultColHeight;
//     private float defaultColCenter;

//     [HideInInspector] public float curCamHeight;
//     [HideInInspector] public float curColHeight;
//     [HideInInspector] public float curColCenter;

//     [Space(10)]

//     public float crouchTargetCamHeight;
//     public float crouchTargetColHeight;
//     public float crouchTargetColCenter;

//     public AnimationCurve crouchCurve;

//     [Space(10)]

//     [Tooltip("Add onto down and up force")] public float crouchForcesAddPerSlam;
//     public float timeToResetCrouchForces;
//     private float defaultCrouchDownForce;
//     public float crouchDownForce;
//     public float maxCrouchDownForce;

//     private IList<Coroutine> crouchingCorutines = new List<Coroutine>();
//     //--------------------Ground Slam--------------------//
//     [Header("Ground Slam")]
//     public GroundSlamAction groundSlamAction;

//     private float defaultGroundSlamUpForce;
//     public float groundSlamUpForce;
//     public float maxGroundSlamUpForce;

//     private bool allowGroundSlam;
//     private float lastGroundSlamTime;
//     private bool canAddForce;

//     //--------------------Explode--------------------//
//     [Header("Explode")]
//     public float groundSlamExplodeRadius;
//     public float groundSlamExplodeForce;
//     public float groundSlamExplodeUpForce;
//     //--------------------Implode--------------------//
//     [Header("Implode")]
//     public float groundSlamImplodeRadius;
//     public float groundSlamImplodeForce;
//     //--------------------Wall Running--------------------//
//     [Header("Wall Running")]
//     public float moveMultiplier;
//     public float gravityMultiplier;

//     public Vector2 wallAngleMinMax;

//     private bool wallGrounded = false;
//     private ISet<Collider> wallColliders = new HashSet<Collider>();
//     //--------------------Dash--------------------//
//     [Header("Dash")]
//     public float dashForce;
//     public float dashDelay;
//     public float dashTime;
//     public float leaveVelocityMultiplier;

//     private float lastDashTime;
//     //--------------------References--------------------//
//     [Header("References")]
//     public TextMeshProUGUI speedText;
//     private CapsuleCollider capsuleCollider;
//     private Transform cameraContainer;
//     private Transform weaponsContainer;
//     private new Transform camera;
//     // private Animator camAnim;
//     private Rigidbody rig;
//     private PhysicMaterial curPhyMat;
//     public AnimationScript animController;
//     //--------------------Weapons--------------------//
//     [Header("Weapons")]
//     public GameObject[] gunInventory;
//     private GameObject[] gunScripts;
//     private PlayerInput playerInput;
//     private InputAction fire;
//     //--------------------Effects--------------------//
//     [Header("Effects")]
//     public GameObject groundSlamParticleSystem;

//     private Collision lastCollision;

//     private WallSide wallSide = WallSide.None;

//     private float lastGroundedTime;
    
//     #region Start

//     void Awake() // Called Before First Frame
//     {
//         InitializeComponents();
//         SetDefaults();
//     }
    
//     void Start() // Called On First Frame
//     {
//         Cursor.lockState = CursorLockMode.Locked;
//         // Every 3 Seconds, Check for input change
//         InvokeRepeating(nameof(CheckIfInputChange), 0.0f, 3f);
        
//         int counter = 0;

//         GameObject weaponContainer = cameraContainer.transform.GetChild(1).gameObject;

//         foreach (GameObject gunObject in gunInventory)
//         {
//             GameObject curGun = Instantiate(gunObject, weaponContainer.transform);
//             GunScript curGunScript = curGun.GetComponent<GunScript>();
//             curGun.transform.localPosition = curGunScript.weaponPos[counter];
//             curGunScript.positionIndex = counter;
//             counter += 1;
//         }

//         gunScripts = GameObject.FindGameObjectsWithTag("Gun");
//     }

//     void InitializeComponents() // Called In Awake(). To Define References
//     {
//         playerInput = GetComponent<PlayerInput>();
//         rig = GetComponent<Rigidbody>();
//         capsuleCollider = GetComponent<CapsuleCollider>();

//         cameraContainer = GameObject.Find("Camera Container").transform;

//         weaponsContainer = GameObject.Find("Weapons Holder").transform;

//         camera = GameObject.Find("Camera").transform;

//         fire = playerInput.actions["Shoot"];

//         curPhyMat = gameObject.GetComponent<Collider>().material;
//     }

//     void SetDefaults() // Called In Awake(). To Define Default Values
//     {
//         defaultCamHeight = cameraContainer.transform.localPosition.y;
//         defaultColHeight = capsuleCollider.height;
//         defaultColCenter = capsuleCollider.center.y;

//         curCamHeight = defaultCamHeight;
//         curColHeight = defaultColHeight;
//         curColCenter = defaultColCenter;

//         defaultMoveForce = moveForce;
//         defaultCrouchDownForce = crouchDownForce;
//         defaultGroundSlamUpForce = groundSlamUpForce;

//         defaultCayoteTime = cayoteTime;
//     }

//     void CheckIfInputChange() // First Called In Start(). Called Every 3 Seconds. To Check Which Input Is Being Used
//     {
//         if (Gamepad.all.Count > 0)
//             lookSenseToUse = controllerLookSens;
//         else
//             lookSenseToUse = mouseLookSens;
//     }

//     #endregion

//     #region Update

//     void FixedUpdate() // Called at Fixed Interval. Frame-Rate Independant
//     {
//         ApplyGravity();
//         Move();
//         ResetAirJumps();
//         WalkingAnimAndMoveForce();

//         GroundedDisableSlam();
//         UpdateCrouchingThings();
//         // Speedometer Text
//         speedText.text = "Current Speed: " + Mathf.RoundToInt(new Vector3(currentXSpeed, currentYSpeed, currentZSpeed).magnitude) + "m/s";
//     }

//     void LateUpdate() // Called After All Other Update Functions
//     {
//         Camera();
//     }

//     void ApplyGravity() // Called in Fixed Update(). Applies Gravity to Player.
//     {
//         if (wallGrounded)
//         {
//             rig.AddForce(Vector3.down * gravityForce * gravityMultiplier, ForceMode.Acceleration);
//         }
//         else
//             rig.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
//         if (isCrouching && !(grounded && wallGrounded) && canAddForce)
//         {
//             rig.AddForce(Vector3.down * crouchDownForce, ForceMode.Acceleration);
//         }
//     }

//     void Move() // Called In Fixed Update(). Adds Movement Forces
//     {
//         currentYSpeed = rig.velocity.y;
//         currentXSpeed = rig.velocity.x;
//         currentZSpeed = rig.velocity.z;
//         // Define player movement
//         Vector3 move = transform.forward * moveValue.y + transform.right * moveValue.x;
//         // Multiply Move Acceleration
//         // Multiply Move Acceleration
//         move *= moveForce;
//         if (wallGrounded)
//         {
//             move *= moveMultiplier;
//         }
        
//         rig.AddForce(move, ForceMode.Acceleration); 
//     }

//     void ResetAirJumps() // Called In Fixed Update(). To Reset AirJumps when Grounded
//     {
//         if (grounded || wallGrounded)
//         {
//             remainingAirJumps = maxAirJumps;
//         }
//     }

//     void WalkingAnimAndMoveForce() // Called In Fixed Update(). Plays Walking Anim And Increases Move Force Over Time.
//     {
//         Vector3 horizontalVelocity = new(rig.velocity.x, 0, rig.velocity.z);

//         bool isMoving = horizontalVelocity.magnitude > movementThreshold;

//         if (isMoving)
//         {
//             animController.StartWalk();

//             if (horizontalVelocity.magnitude < absoluteMaxSpeed)
//             {
//                 moveForce += Time.fixedDeltaTime * moveForceIncreasePerSecond;
//             }
//         }
//         else if (!isMoving)
//         {
//             moveForce = defaultMoveForce;
//         }
//     }

//     void GroundedDisableSlam() // Called In Fixed Update(). Disables Ground Slam While Grounded
//     {
//         if (grounded || wallGrounded)
//         {
//             allowGroundSlam = false;
//             canAddForce = false;
//         }   
//     }

//     void UpdateCrouchingThings() // Called In Fixed Update(). Sets Values Used In Crouching Coroutines
//     {
//         cameraContainer.transform.localPosition = Vector3.up * curCamHeight;
//         capsuleCollider.height = curColHeight;
//         capsuleCollider.center = Vector3.up * curColCenter;
//     }
    
//     void Camera() // Called In Late Update(). Rotates Camera and Player as Needed
//     {
//         // Rotate the Camera
//         curCameraX += deltaMouseValue.y * lookSenseToUse;
//         curCameraX = Mathf.Clamp(curCameraX, minX, maxX);

//         cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

//         transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
//     }

//     #endregion

//     #region Input

//     public void OnLookInput(InputAction.CallbackContext context)
//     {
//         // Stores Delta Look Value
//         deltaMouseValue = context.ReadValue<Vector2>();
//     }

//     public void OnMoveInput(InputAction.CallbackContext context)
//     {
//         if (context.phase == InputActionPhase.Performed)
//         {
//             moveValue = context.ReadValue<Vector2>();
//         }
//         else if (context.phase == InputActionPhase.Canceled)
//         {
//             moveValue = Vector2.zero;
//         }
//     }

//     public void OnJumpInput(InputAction.CallbackContext context)
//     {
//         // If this is the frame we jump
//         if (context.phase == InputActionPhase.Performed)
//         {
//             jumpHeld = true;
//             Jump();
//         }
//         if (context.phase == InputActionPhase.Canceled)
//             jumpHeld = false;
//     }

//     public void OnCrouchInput(InputAction.CallbackContext context)
//     {
//         // If we crouch on this frame, isCrouching true. 
//         if (context.phase == InputActionPhase.Performed)
//         {
//             if (crouchType == CrouchType.Hold)
//             {
//                 Crouch();
//             }
//             else if (!isCrouching)
//             {
//                 Crouch();
//             }
//             else
//             {
//                 UnCrouch();
//             }
//         }
//         else if (context.phase == InputActionPhase.Canceled && crouchType == CrouchType.Hold)
//         {
//             UnCrouch();
//         }
//     }
    
//     public void OnExitInput(InputAction.CallbackContext context)
//     {
//         if (context.phase == InputActionPhase.Performed)
//             Application.Quit();
//     }

//     public void OnGrappleInput(InputAction.CallbackContext context)
//     {
//         if (context.phase == InputActionPhase.Performed)
//         {

//         }
//         else if (context.phase == InputActionPhase.Canceled)
//         {
            
//         }
//     }

//     public void OnDashInput(InputAction.CallbackContext context)
//     {
//         if (context.phase == InputActionPhase.Performed && lastDashTime + dashDelay <= Time.time)
//         {
//             lastDashTime = Time.time;
//             StartCoroutine(Dash());
//         }
//     }

//     #endregion
    
//     #region Jumping/Collision

//     void OnCollisionEnter(Collision other)
//     {
//         Debug.Log("Collision Enter");
//         // Ground Checks
//         GroundCheck(other);
//         WallGroundCheck(other);

//         HoldingJumpButton();

//         lastCollision = other;

//         cayoteTime = defaultCayoteTime;
//     }

//     void GroundCheck(Collision other) // Called In OnCollisionEnter(). Checks If Any Collision Points at FeetYLevel
//     {
//         var contactPoints = new ContactPoint[other.contactCount];
//         other.GetContacts(contactPoints);

//         var feetYLevel = (transform.position.y + curColCenter) - curColHeight / 2;

//         for (int i = 0; i < contactPoints.Length; i++)
//         {
//             if (Mathf.Abs(contactPoints[i].point.y - feetYLevel) < feetTolerance)
//             {
//                 groundColiders.Add(other.collider);
//                 break;
//             }
//         }

//         if (!grounded && groundColiders.Any())
//         {
//             LandingAnimations(other);
//         }

//         grounded = groundColiders.Any();
//     }

//     void WallGroundCheck(Collision other)
//     {
//         if (grounded || isCrouching)
//             return;

//         var contactPoints = new ContactPoint[other.contactCount];
//         other.GetContacts(contactPoints);

//         foreach (ContactPoint contactPoint in contactPoints)
//         {
//             float groundAngle = Vector3.Angle(contactPoint.normal, Vector3.up);
            
//             if (groundAngle >= wallAngleMinMax.x && groundAngle <= wallAngleMinMax.y)
//             {
//                 // Get dot product of contact normal and player's forward/right/back/left vectors
//                 Vector3 normal = contactPoint.normal;

//                 float dotForward = Vector3.Dot(normal, transform.forward);
//                 float dotRight = Vector3.Dot(normal, transform.right);
//                 float dotBack = Vector3.Dot(normal, -transform.forward);
//                 float dotLeft = Vector3.Dot(normal, -transform.right);

//                 if (dotForward > 0.5f)
//                 {
//                     wallColliders.Add(other.collider);

//                     wallSide = WallSide.Back;
//                 }
//                 else if (dotRight > 0.5f)
//                 {
//                     wallColliders.Add(other.collider);

//                     wallSide = WallSide.Left;
//                 }
//                 else if (dotBack > 0.5f)
//                 {
//                     wallColliders.Add(other.collider);

//                     wallSide = WallSide.Front;
//                 }
//                 else if (dotLeft > 0.5f)
//                 {
//                     wallColliders.Add(other.collider);

//                     wallSide = WallSide.Right;
//                 }
//                 else
//                 {
//                     wallSide = WallSide.None;
//                 }
//             }
//         }

//         if (!wallGrounded && wallColliders.Any())
//         {
//             WallRunningIn();
//         }

//         wallGrounded = wallColliders.Any();
//     }

//     void HoldingJumpButton() // Called In OnCollisionEnter(). Jumps if Button Is Held
//     {
//         if (jumpHeld && grounded)
//         {
//             Jump();
//         }
//     }
        
//     void LandingAnimations(Collision other) // Called In OnCollisionEnter(). If Not Grounded Before, But Grounded Now. Play Animations and GroundSlam
//     {
//         animController.Land();

//         if (isCrouching && allowGroundSlam)
//         {
//             Instantiate(groundSlamParticleSystem, other.GetContact(0).point, Quaternion.identity);

//             if (Time.time - lastGroundSlamTime <= timeToResetCrouchForces)
//             {
//                 crouchDownForce += crouchForcesAddPerSlam;
//                 crouchDownForce = Mathf.Clamp(crouchDownForce, 0, maxCrouchDownForce);
//                 groundSlamUpForce += crouchForcesAddPerSlam;
//                 groundSlamUpForce = Mathf.Clamp(groundSlamUpForce, 0, maxGroundSlamUpForce);
//             }
//             else
//             {
//                 crouchDownForce = defaultCrouchDownForce;
//                 groundSlamUpForce = defaultGroundSlamUpForce;
//             }

//             lastGroundSlamTime = Time.time;

//             if (moveValue.x > 0)
//             {
//                 animController.Slam(2);
//                 rig.AddForce(other.GetContact(0).normal * groundSlamUpForce, ForceMode.Impulse);
//                 allowGroundSlam = false;
//             }
//             else if (moveValue.x < 0)
//             {
//                 animController.Slam(0);
//                 rig.AddForce(other.GetContact(0).normal * groundSlamUpForce, ForceMode.Impulse);
//                 allowGroundSlam = false;
//             }
//             else
//             {
//                 animController.Slam(1);
//                 rig.AddForce(other.GetContact(0).normal * groundSlamUpForce, ForceMode.Impulse);
//                 allowGroundSlam = false;
//             }
//             if (groundSlamAction == GroundSlamAction.Explode)
//                 Algorithms.Explode(transform.position, groundSlamExplodeRadius, groundSlamExplodeForce, groundSlamUpForce);
//             else
//                 Algorithms.Implode(transform.position, groundSlamImplodeRadius, groundSlamImplodeForce);
//         }
//     }

//     void WallRunningIn()
//     {
//         switch (wallSide)
//         {
//             case WallSide.Right:
//                 WallRunRightIn();
//                 break;

//             case WallSide.Left:
//                 WallRunLeftIn();
//                 break;
//         }  
//     }

//     void WallRunningOut()
//     {
//         switch (wallSide)
//         {
//             case WallSide.Right:
//                 WallRunRightOut();
//                 break;

//             case WallSide.Left:
//                 WallRunLeftOut();
//                 break;
//         }
//     }

//     void Jump() // Called in OnJumpInput() On First Frame. Determines Direction to Jump In
//     {
//         bool haveCayoteTimeLeft = Time.time <= lastGroundedTime + cayoteTime;

//         if ((grounded || haveCayoteTimeLeft) && !wallGrounded)
//         {
//             AddJumpForce(Vector3.up);
//         }
//         else if (wallGrounded || haveCayoteTimeLeft)
//         {
//             // WallGroundCheck(lastCollision);
            
//             switch (wallSide)
//             {
//                 case WallSide.Front:
//                     AddJumpForce(Vector3.up + -transform.forward);
//                     break;

//                 case WallSide.Back:
//                     AddJumpForce(Vector3.up + transform.forward);
//                     break;

//                 case WallSide.Right:
//                     AddJumpForce(Vector3.up + -transform.right);
//                     break;

//                 case WallSide.Left:
//                     AddJumpForce(Vector3.up + transform.right);
//                     break;
//             }
//         }
//         else if (remainingAirJumps > 0)
//         {
//             AddJumpForce(Vector3.up);
//             remainingAirJumps -= 1;
//         }
//     }

//     void AddJumpForce(Vector3 direction) // Called In Jump(). Adds A Force In direction
//     {
//         rig.AddForce(direction * jumpForce, ForceMode.Impulse);
//         animController.Jump();
//         cayoteTime = 0;
//     }


//     void OnCollisionExit(Collision other)
//     {
//         Debug.Log("Collision Exit");
//         // Check if Still Grounded
//         groundColiders.Remove(other.collider);

//         grounded = groundColiders.Any();

//         // Check if Still WallGrounded
//         wallColliders.Remove(other.collider);

//         if (wallGrounded && !wallColliders.Any())
//         {
//             WallRunningOut();
//         }

//         wallGrounded = wallColliders.Any();

//         lastGroundedTime = Time.time;
//     }

//     #endregion

//     #region OtherFunctions
//     private IEnumerator Dash() // Called In OnDashInput(). Adds Force, Waits dashTime. Then Resets player velocity to starting velocity * leaveVelocityMultiplier
//     {
//         Vector3 startVelocity = new(currentXSpeed, 0, currentZSpeed);
//         Vector3 currentMoveValue = new(moveValue.normalized.x, 0, moveValue.normalized.y);

//         if (currentMoveValue.x > 0.2f)
//         {
//             animController.Dash(2);
//             rig.AddRelativeForce(currentMoveValue* dashForce, ForceMode.Impulse);
//         }
//         else if (currentMoveValue.x < -0.2f)
//         {
//             animController.Dash(3);
//             rig.AddRelativeForce(currentMoveValue* dashForce, ForceMode.Impulse);
//         }

//         if (currentMoveValue.z > 0.2f)
//         {
//             animController.Dash(0);
//             rig.AddForce(cameraContainer.forward * dashForce, ForceMode.Impulse);
//         }
//         else if (currentMoveValue.z < -0.2f)
//         {
//             animController.Dash(1);
//             rig.AddForce(-cameraContainer.forward * dashForce, ForceMode.Impulse);
//         }

//         yield return new WaitForSeconds(dashTime);
//         rig.velocity = startVelocity * leaveVelocityMultiplier;
//     }

//     private void Crouch() // Called In OnCrouchInput(). Starts A Bunch Of Coroutines to Move the Player
//     {
//         if (!grounded) 
//         {
//             allowGroundSlam = true;
//             canAddForce = true;
//         }
//         isCrouching = true;
//         Debug.Log("Crouch");
//         StopCrouchingCoroutines();
//         crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, crouchTargetCamHeight, crouchCurve, crouchTransitionTime)));
//         crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, crouchTargetColCenter, crouchCurve, crouchTransitionTime)));
//         crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, crouchTargetColHeight, crouchCurve, crouchTransitionTime)));
//     }

//     private void UnCrouch() // Called In OnCrouchInput(). Starts A Bunch Of Coroutines to Move the Player
//     {
//         isCrouching = false;
//         canAddForce = false;
//         Debug.Log("UnCrouch");
//         StopCrouchingCoroutines();
//         crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curCamHeight", curCamHeight, defaultCamHeight, crouchCurve, crouchTransitionTime)));
//         crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColCenter", curColCenter, defaultColCenter, crouchCurve, crouchTransitionTime)));
//         crouchingCorutines.Add(StartCoroutine(Algorithms.CurveLerp(this, "curColHeight", curColHeight, defaultColHeight, crouchCurve, crouchTransitionTime)));
//     }

//     void StopCrouchingCoroutines()
//     {
//         foreach(var coroutine in crouchingCorutines)
//         {
//             StopCoroutine(coroutine);
//         }

//         crouchingCorutines.Clear();
//     }

//     void WallRunRightIn()
//     {
//         animController.WallRunIn(0);
//     }

//     void WallRunRightOut()
//     {
//         animController.WallRunOut(0);
//     }

//     void WallRunLeftIn()
//     {
//         animController.WallRunIn(1);
//     }

//     void WallRunLeftOut()
//     {
//         animController.WallRunOut(1);
//     }
//     #endregion
// }