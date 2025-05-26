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
    InputType inputType = InputType.Keyboard;

    [Header("Look Sensitivity")]

    [SerializeField] [Range(0, 100)] float controllerLookSens;
    [SerializeField] [Range(0, 10)] float mouseLookSens; 
    float lookSenseToUse;

    [Header("Look Range")]
    [SerializeField] [MinMaxSlider(-180, 180)] Vector2 lookXRange;
    float curCameraX;
    Vector2 deltaMouseValue;

    [Header("Containers")]
    [SerializeField] Transform aimingCameraContainer;
    [SerializeField] Transform aimingWeaponContainer;

    void Start()
    {
        // is there a gamepad connected, if so use controllerLookSens, else mouseLookSens
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

        // lock cursor to window        
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnLookInput(InputAction.CallbackContext context)
    {
        // store mouse move value
        deltaMouseValue = context.ReadValue<Vector2>();
    }

    void LateUpdate()
    {
        curCameraX += deltaMouseValue.y * lookSenseToUse;

        curCameraX = Mathf.Clamp(curCameraX, lookXRange.x, lookXRange.y);

        // rotate aimingCameraContainer up/down
        aimingCameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);
        aimingWeaponContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);
        // rotate player left/right
        transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
    }
}
