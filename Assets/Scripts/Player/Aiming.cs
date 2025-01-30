using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Aiming : PlayerMain
{
    public float controllerLookSens;
    public float mouseLookSens;

    private float lookSenseToUse;

    [Tooltip("Up and Down Rotation")] public float maxX;
    [Tooltip("Up and Down Rotation")] public float minX;

    public float curCameraX;

    Vector2 deltaMouseValue;

    void Start()
    {
        if (Gamepad.all.Count > 0)
        {
            lookSenseToUse = controllerLookSens;
            inputType = InputType.Controller;
        }
        else
        {
            lookSenseToUse = mouseLookSens;
            inputType = InputType.Keyboard;
        }
    }
    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Stores Delta Look Value
        deltaMouseValue = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Rotate the Camera
        curCameraX += deltaMouseValue.y * lookSenseToUse;
        curCameraX = Mathf.Clamp(curCameraX, minX, maxX);

        cameraContainer.localEulerAngles = new Vector3(-curCameraX, 0, 0);

        transform.eulerAngles += new Vector3(0, deltaMouseValue.x * lookSenseToUse, 0);
    }
}
