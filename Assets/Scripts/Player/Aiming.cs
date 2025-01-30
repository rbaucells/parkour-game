using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Aiming : MonoBehaviour
{
    [SerializeField] PlayerMain playerMain; 

    [SerializeField] [Range(0, 100)] float controllerLookSens;
    [SerializeField] [Range(0, 10)] float mouseLookSens; 

    float lookSenseToUse; // Depends on inputType

    [SerializeField] [Range(0, 180)] float maxX;
    [SerializeField] [Range(-180, 0)] float minX;

    [SerializeField] Transform cameraContainer;

    Vector2 deltaMouseValue; // Look Input from Player
    float curCameraX; // Current X Rotation of Camera

    void Start()
    {
        // Set Look Sensitivity depending on inputType
        switch (playerMain.inputType)
        {
            case PlayerMain.InputType.Keyboard:
                lookSenseToUse = mouseLookSens;
                break;
            case PlayerMain.InputType.Controller:
                lookSenseToUse = controllerLookSens;
                break;
        }
        // Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Store Delta Look Value
        deltaMouseValue = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Add Delta Mouse Value to previous Value
        curCameraX += deltaMouseValue.y * lookSenseToUse;

        // Clamp it to MinMax
        curCameraX = Mathf.Clamp(curCameraX, minX, maxX);

        // Rotate CameraContainer Up/Down
        cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

        // Rotate player Left/Right
        transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
    }
}
