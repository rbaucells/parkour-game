using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class Aiming : MonoBehaviour
{
    public enum InputType
    {
        Keyboard,
        Controller
    }

    [SerializeField] [Range(0, 100)] float controllerLookSens;
    [SerializeField] [Range(0, 10)] float mouseLookSens; 

    float lookSenseToUse; // Depends on inputType

    [SerializeField] [MinMaxSlider(-180, 180)] Vector2 lookXRange; // X Rotation Range

    [SerializeField] Transform cameraContainer;

    InputType inputType = InputType.Keyboard;

    Vector2 deltaMouseValue; // Look Input from Player
    float curCameraX; // Current X Rotation of Camera

    void Start()
    {
        // Set Look Sensitivity depending on inputType
        if (Gamepad.current != null)
        {
            inputType = InputType.Controller;
            lookSenseToUse = controllerLookSens;
        }
        else
        {
            inputType = InputType.Keyboard;
            lookSenseToUse = mouseLookSens;
        }
        // Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Store Delta Look Value
        deltaMouseValue = context.ReadValue<Vector2>();
    }

    void LateUpdate()
    {
        // Add Delta Mouse Value to previous Value
        curCameraX += deltaMouseValue.y * lookSenseToUse;

        // Clamp it to MinMax
        curCameraX = Mathf.Clamp(curCameraX, lookXRange.x, lookXRange.y);

        // Rotate CameraContainer Up/Down
        cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

        // Rotate player Left/Right
        transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
    }
}
